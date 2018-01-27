using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Wayless.ExpressionBuilders;

namespace Wayless.Generic
{
    public sealed class Wayless<TDestination, TSource>
        : IWayless<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        /// Type activator. Using static compiled expression for improved performance
        private static readonly Func<TDestination> _destinationActivator = Expression.Lambda<Func<TDestination>>(Expression.New(typeof(TDestination)
                                                                              .GetConstructor(Type.EmptyTypes)))
                                                                              .Compile();

        /// <summary>
        /// Generates mapping expressions that will be eventually compiled into map
        /// </summary>
        private readonly ExpressionBuilder _expressionBuilder = new ExpressionBuilder(typeof(TDestination), typeof(TSource));

        private readonly IDictionary<string, MemberInfo> _destinationFields;
        private readonly IDictionary<string, MemberInfo> _sourceFields;

        private readonly IList<string> _fieldSkips;
        private readonly IDictionary<string, Expression> _fieldExpressions;

        // Indicates if the _compiledMap action has the latest rules
        // each time mapping is modified this flag will be switched
        // to false, letting Wayless know to compile a new 
        // mapping function from it's collected expressions
        private bool _isMapUpToDate;

        // flag indicating if mapper has tried matching pairs on its own
        private bool _hasMatchedDefaults;

        private Action<TDestination, TSource> _map;

        /// <summary>
        /// Create instance of Wayless mapper
        /// </summary>
        /// <param name="ignoreCasing">
        ///     True: Ignores casing during  initial property matchup
        ///     False: Property name casing affects matching
        /// </param>
        public Wayless()
        {
            DestinationType = typeof(TDestination);
            SourceType = typeof(TSource);

            _isMapUpToDate = false;

            _fieldExpressions = new Dictionary<string, Expression>();
            _fieldSkips = new List<string>();
            _destinationFields = DestinationType.ToMemberInfoDictionary();
            _sourceFields = SourceType.ToMemberInfoDictionary();
        }

        /// <summary>
        /// Type to map from
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Type to map to
        /// </summary>
        public Type DestinationType { get; }

        /// <summary>
        /// Apply mapping rules to  existing instance of object
        /// </summary>
        /// <param name="destinationObject">Object to apply mapping rules to</param>
        /// <param name="sourceObject">Object to read values from</param>
        public void Map(TDestination destinationObject, TSource sourceObject)
        {
            InternalMap(destinationObject, sourceObject);
        }

        /// <summary>
        /// Apply mapping using a collection of source objects. Each object in the 
        /// source list will be mapped to a corresponding object in the output list
        /// </summary>
        /// <param name="sourceList">List of object sto map</param>
        /// <param name="constructorParameters">Constructor parameters, if any, to be used when 
        /// creating an instance of the destination object</param>
        /// <returns>Collection of mapped objects</returns>
        public IEnumerable<TDestination> Map(IEnumerable<TSource> sourceList)
        {
            foreach (var sourceObject in sourceList)
            {
                yield return Map(sourceObject);
            }
        }

        /// <summary>
        /// Apply mapping rules in source object
        /// </summary>
        /// <param name="sourceObject">Object to read avalues from</param>
        /// <param name="constructorParameters">Parameters passed to destination object constructor</param>
        /// <returns>Mapped object</returns>
        public TDestination Map(TSource sourceObject)
        {
            TDestination destinationObject = _destinationActivator();

            InternalMap(destinationObject, sourceObject);

            return destinationObject;
        }

        /// <summary>
        /// Create a mapping rule between destination property and 
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="sourceExpression">Expression for property to be read in source type</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWayless<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression
                                                      , Expression<Func<TSource, object>> sourceExpression)
        {
            FieldMap(destinationExpression, sourceExpression, null);

            return this;
        }

        /// <summary>
        /// Create a mapping rule between destination property and 
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="sourceExpression">Expression for property to be read in source type</param>
        /// <param name="mapCondition">Conditition to be evaluated for determening if values should be mapped</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWayless<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression
                                                      , Expression<Func<TSource, object>> sourceExpression
                                                      , Expression<Func<TSource, bool>> mapCondition)
        {
            var destination = GetInvariantName(destinationExpression);

            if (!_fieldSkips.Contains(destination))
            {
                var expression = _expressionBuilder.GetMapExpression(destinationExpression, sourceExpression, mapCondition);

                RegisterFieldExpression(destination, expression);

                _isMapUpToDate = false;
            }

            return this;
        }

        /// <summary>
        /// Create mapping rule to explicitly assign value to destination property
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="value">Value to assign</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWayless<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object value)
        {
            var destination = GetInvariantName(destinationExpression);
            if (!_fieldSkips.Contains(destination))
            {
                var expression = _expressionBuilder.GetMapExressionForExplicitSet(destinationExpression, value);

                RegisterFieldExpression(destination, expression);

                _isMapUpToDate = false;
            }
            return this;
        }

        public IWayless<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression
                                                      , object value
                                                      , Expression<Func<TSource, bool>> setCondition)
        {
            var destination = GetInvariantName(destinationExpression);
            if (!_fieldSkips.Contains(destination))
            {
                var expression = _expressionBuilder.GetMapExressionForExplicitSet(destinationExpression, value, setCondition);

                RegisterFieldExpression(destination, expression);

                _isMapUpToDate = false;
            }
            return this;
        }
        /// <summary>
        /// Create mapping rule for properties to be skipped in destination type
        /// </summary>
        /// <param name="ignoreAtDestinationExpression">Expression for property to be skipped in destination type</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWayless<TDestination, TSource> FieldSkip(Expression<Func<TDestination, object>> skipperName)
        {
            var ignore = GetInvariantName(skipperName);
            if (_fieldExpressions.ContainsKey(ignore))
            {
                _fieldExpressions.Remove(ignore);
                if (!_fieldSkips.Contains(ignore))
                {
                    _fieldSkips.Add(ignore);
                }
            }

            _isMapUpToDate = false;
            return this;
        }

        /// <summary>
        /// If a field was set to be skipped, this method will restore the mapping to a default state if one existed
        /// </summary>
        /// <param name="restoreAtDestinationExpression"></param>
        /// <returns></returns>
        public IWayless<TDestination, TSource> FieldRestore(Expression<Func<TDestination, object>> restoreAtDestinationExpression)
        {
            var restore = restoreAtDestinationExpression.GetMemberInfo();
            var key = restore.Name.ToLowerInvariant();
            if (_fieldSkips.Contains(key))
            {
                _fieldSkips.Remove(key);
                FindAndSetDestinationToSource(new KeyValuePair<string, MemberInfo>(key, restore));
                _isMapUpToDate = false;
            }

            return this;
        }


        #region helpers
        private void RegisterFieldExpression(string destination, Expression expression)
        {
            if (_fieldExpressions.ContainsKey(destination))
            {
                _fieldExpressions[destination] = expression;
            }
            else
            {
                _fieldExpressions.Add(destination, expression);
            }
        }

        // apply all mapping rules
        private void InternalMap(TDestination destinationObject, TSource sourceObject)
        {
            if (!_isMapUpToDate)
            {
                if (!_hasMatchedDefaults)
                {
                    MatchUnmappedItems();
                    _hasMatchedDefaults = true;
                }

                _map = _expressionBuilder.BuildActionMap<TDestination, TSource>(_fieldExpressions.Values);
                _isMapUpToDate = true;
            }

            _map(destinationObject, sourceObject);
        }

        // try to automatically map any unmapped members
        private void MatchUnmappedItems()
        {
            var unmappedDestinations = _destinationFields.Where(x => !_fieldExpressions.Keys.Contains(x.Key)
                                                                  && !_fieldSkips.Contains(x.Key)).ToList();

            foreach (var destination in unmappedDestinations)
            {
                FindAndSetDestinationToSource(destination);
            }
        }

        private void FindAndSetDestinationToSource(KeyValuePair<string, MemberInfo> destination)
        {
            if (_sourceFields.TryGetValue(destination.Key, out MemberInfo source))
            {
                var expression = _expressionBuilder.GetMapExpression<TSource>(destination.Value, source);
                _fieldExpressions.Add(destination.Key, expression);
            }
        }

        // get property name
        private static string GetInvariantName<T>(Expression<Func<T, object>> expression)
            where T : class
        {
            var propertyInfo = expression.GetMemberInfo();

            return propertyInfo.Name.ToLowerInvariant();
        }

        #endregion helpers
    }
}
