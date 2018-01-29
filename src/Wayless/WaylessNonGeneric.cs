using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Wayless.ExpressionBuilders;
using Wayless.Generic;

namespace Wayless
{
    public class Wayless 
        : IWayless
    {
        /// Type activator. Using static compiled expression for improved performance
        private readonly Func<object> _instanceCreate;

        /// <summary>
        /// Generates mapping expressions that will be eventually compiled into map
        /// </summary>
        private readonly ExpressionBuilder _expressionBuilder;

        private readonly IDictionary<string, MemberInfo> _destinationFields;
        private readonly IDictionary<string, MemberInfo> _sourceFields;

        private readonly IList<string> _fieldSkips;
        private readonly IDictionary<string, Expression> _fieldExpressions;

        // Indicates if the _compiledMap action has the latest rules
        // each time mapping is modified this flag will be switched
        // to false, letting Wayless know to compile a new 
        // mapping function from it's collected expressions
        private bool _isMapUpToDate;
        private bool _attemptAutoMatchMembers;

        private Action<object, object> _map;

        public Wayless(Type destinationType, Type sourceType)
        {
            DestinationType = destinationType;
            SourceType = sourceType;

            _instanceCreate = Extensions.CreateInstanceLambda(destinationType);
            _expressionBuilder = new ExpressionBuilder(destinationType, sourceType);

            _isMapUpToDate = false;
            _attemptAutoMatchMembers = true;

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
        public void Map<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class
        {
            IsGenericTypeOfRequest<TDestination, TSource>();
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
        public IEnumerable<TDestination> Map<TDestination, TSource>(IEnumerable<TSource> sourceList)
            where TDestination : class
            where TSource : class
        {
            foreach (var sourceObject in sourceList)
            {
                yield return Map<TDestination, TSource>(sourceObject);
            }
        }

        /// <summary>
        /// Apply mapping rules in source object
        /// </summary>
        /// <param name="sourceObject">Object to read avalues from</param>
        /// <param name="constructorParameters">Parameters passed to destination object constructor</param>
        /// <returns>Mapped object</returns>
        public TDestination Map<TDestination, TSource>(TSource sourceObject)
            where TDestination : class
            where TSource : class
        {
            TDestination destinationObject = (TDestination)_instanceCreate();

            InternalMap(destinationObject, sourceObject);

            return destinationObject;
        }

        /// <summary>
        /// Create a mapping rule between destination property and 
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="sourceExpression">Expression for property to be read in source type</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWayless FieldMap<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression
                                             , Expression<Func<TSource, object>> sourceExpression)
            where TDestination : class
            where TSource : class
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
        public IWayless FieldMap<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression
                                                      , Expression<Func<TSource, object>> sourceExpression
                                                      , Expression<Func<TSource, bool>> mapCondition)
            where TDestination : class
            where TSource : class
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
        public IWayless FieldSet<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, object value)
            where TDestination : class
            where TSource : class
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

        public IWayless FieldSet<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression
                                                      , object value
                                                      , Expression<Func<TSource, bool>> setCondition)
            where TDestination : class
            where TSource : class
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
        public IWayless FieldSkip<TDestination, TSource>(Expression<Func<TDestination, object>> skipperName)
            where TDestination : class
            where TSource : class
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

        #region basic mapper configuration

        public IWayless DontAutoMatchMembers()
        {
            _attemptAutoMatchMembers = false;

            return this;
        }

        #endregion basic mapper configuration
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
        private void InternalMap<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class
        {
            IsGenericTypeOfRequest<TDestination, TSource>();
            CompileMapFunction<TDestination, TSource>();

            _map(destinationObject, sourceObject);
        }

        private void CompileMapFunction<TDestination, TSource>()
            where TDestination : class
            where TSource : class
        {
            if (!_isMapUpToDate)
            {
                if (_attemptAutoMatchMembers)
                {
                    AutomatchMembers<TDestination, TSource>();
                }

                _map = new Action<object, object>((d,s) => _expressionBuilder.BuildActionMap<TDestination, TSource>(_fieldExpressions.Values).Invoke((TDestination)d, (TSource)s));
                _isMapUpToDate = true;
            }
        }

        // try to automatically map any unmapped members
        private void AutomatchMembers<TDestination, TSource>()
            where TDestination : class
            where TSource : class
        {
            var unmappedDestinations = _destinationFields.Where(x => !_fieldExpressions.Keys.Contains(x.Key)
                                                                  && !_fieldSkips.Contains(x.Key)).ToList();

            foreach (var destination in unmappedDestinations)
            {
                FindAndSetDestinationToSource<TSource>(destination);
            }
        }

        private void FindAndSetDestinationToSource<TSource>(KeyValuePair<string, MemberInfo> destination)
            where TSource : class
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

        private void IsGenericTypeOfRequest<TDestination, TSource>()
        {
            if (!typeof(TDestination).Equals(DestinationType))
                throw new TypeInitializationException(typeof(TDestination).FullName, new Exception($"Submitted generic type does not equal initialized constructor type. Expected type {DestinationType.FullName}"));

            if (!typeof(TSource).Equals(SourceType))
                throw new TypeInitializationException(typeof(TSource).FullName, new Exception($"Submitted generic type does not equal initialized constructor type. Expected type {SourceType.FullName}"));
        }
        #endregion helpers
    }
}
