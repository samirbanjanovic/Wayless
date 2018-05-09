using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Wayless.Core;

namespace Wayless
{
    public sealed class Wayless<TDestination, TSource>
        : IWayless<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        /// Type activator. Using static compiled expression for improved performance
        private static readonly Func<TDestination> _createDestinationInstance = Helpers.LambdaCreateInstance<TDestination>();
        private readonly IDictionary<string, MemberInfo> _destinationFields;
        private readonly IDictionary<string, MemberInfo> _sourceFields;
        private readonly IList<string> _fieldSkips;
        private readonly IDictionary<string, Expression> _fieldExpressions;
        private readonly IDictionary<Type, Expression> _typeApplyExpressions;

        /// <summary>
        /// object containing expression builder and field match maker
        /// </summary>
        private readonly IWaylessConfiguration _waylessConfiguration;

        // Indicates if the _compiledMap action has the latest rules
        // each time mapping is modified this flag will be switched
        // to false, letting Wayless know to compile a new 
        // mapping function from it's collected expressions
        private bool _isMapUpToDate;

        private Action<TDestination, TSource> _map;
        
        public Wayless(IWaylessConfiguration waylessConfiguration)
        {
            _waylessConfiguration = waylessConfiguration;

            _isMapUpToDate = false;

            _fieldExpressions = new Dictionary<string, Expression>();
            _fieldSkips = new List<string>();

            _destinationFields = DestinationType.ToMemberInfoDictionary(true);
            _sourceFields = SourceType.ToMemberInfoDictionary();
            _typeApplyExpressions = new Dictionary<Type, Expression>();
        }

        public Wayless()
            : this(WaylessConfigurationBuilder.GetDefaultConfiguration<TDestination, TSource>())
        { }

        /// <summary>
        /// Type to map from
        /// </summary>
        public Type SourceType => typeof(TSource);

        /// <summary>
        /// Type to map to
        /// </summary>
        public Type DestinationType => typeof(TDestination);

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
            IList<TDestination> mappedObjects = new List<TDestination>();
            foreach (var sourceObject in sourceList)
            {
                mappedObjects.Add(Map(sourceObject));
            }

            return mappedObjects;
        }

        /// <summary>
        /// Apply mapping rules in source object
        /// </summary>
        /// <param name="sourceObject">Object to read avalues from</param>
        /// <param name="constructorParameters">Parameters passed to destination object constructor</param>
        /// <returns>Mapped object</returns>
        public TDestination Map(TSource sourceObject)
        {
            TDestination destinationObject = _createDestinationInstance();

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
        /// <param name="condition">Conditition to be evaluated for determening if values should be mapped</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWayless<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression
                                                      , Expression<Func<TSource, object>> sourceExpression
                                                      , Expression<Func<TSource, bool>> condition)
        {
            var destination = GetMemberName(destinationExpression);

            if (!_fieldSkips.Contains(destination))
            {
                var expression = _waylessConfiguration.ExpressionBuilder.GetMapExpression(destinationExpression, sourceExpression, condition);

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
            FieldSet(destinationExpression, value, null);
            return this;
        }

        /// <summary>
        /// Create mapping rule to explicitly assign value to destination property
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="value">Value to assign</param>
        /// <param name="condition">Conditition to be evaluated for determening if values should be set</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWayless<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression
                                                      , object value
                                                      , Expression<Func<TSource, bool>> setCondition)
        {
            var destination = GetMemberName(destinationExpression);
            if (!_fieldSkips.Contains(destination))
            {
                var expression = _waylessConfiguration.ExpressionBuilder.GetMapExressionForExplicitSet(destinationExpression, value, setCondition);

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
            var ignore = GetMemberName(skipperName);

            if (!_fieldSkips.Contains(ignore))
            {
                _fieldSkips.Add(ignore);
            }

            if (_fieldExpressions.ContainsKey(ignore))
            {
                _fieldExpressions.Remove(ignore);
            }

            _isMapUpToDate = false;
            return this;
        }

        public IWayless<TDestination, TSource> TypeApply<T>(Expression<Func<T, T>> typeApplyExpression)
        {
            var typeKey = typeof(T);
            if(_typeApplyExpressions.ContainsKey(typeKey))
            {
                _typeApplyExpressions[typeKey] = typeApplyExpression;
            }
            else
            {
                _typeApplyExpressions.Add(typeKey, typeApplyExpression);
            }

            return this;
        }
        #region helpers
        private IWayless<TDestination, TSource> RegisterFieldExpression(string destination, Expression expression)
        {
            if (_fieldExpressions.ContainsKey(destination))
            {
                _fieldExpressions[destination] = expression;
            }
            else
            {
                _fieldExpressions.Add(destination, expression);
            }

            _isMapUpToDate = false;

            return this;
        }

        // apply all mapping rules
        private void InternalMap(TDestination destinationObject, TSource sourceObject)
        {
            CompileMapFunction();

            _map(destinationObject, sourceObject);
        }

        private void CompileMapFunction()
        {
            if (!_isMapUpToDate)
            {
                if (_waylessConfiguration.MatchMaker != null && _waylessConfiguration.AutoMatchMembers)
                {
                    AutomatchMembers();
                }

                WrapTypeApplyExpressions();

                _map = _waylessConfiguration.ExpressionBuilder
                                            .CompileExpressionMap<TDestination, TSource>(_fieldExpressions.Values);
                _isMapUpToDate = true;
            }
        }

        private void WrapTypeApplyExpressions()
        {
            foreach(var expression in _fieldExpressions.ToArray())
            {
                BinaryExpression binaryExpression = null;
                
                if(expression.Value is BinaryExpression)
                {
                    binaryExpression = expression.Value as BinaryExpression;
                }
        
                var right = binaryExpression.Right;
                if(_typeApplyExpressions.TryGetValue(_destinationFields[expression.Key].GetUnderlyingType(), out Expression value))
                {
                    right = Expression.Invoke(value, right);
                    var assignExpression = Expression.Assign(binaryExpression.Left, right);
                    _fieldExpressions[expression.Key] = assignExpression;
                }                       
            }
        } 

        // try to automatically map any unmapped members
        private void AutomatchMembers()
        {
            var unmappedDestinations = _destinationFields.Where(x => !_fieldExpressions.Keys.Contains(x.Key)
                                                                  && !_fieldSkips.Contains(x.Key))
                                                         .Select(x => x.Value)
                                                         .ToList();
            
            var matchedPairs = _waylessConfiguration.MatchMaker.FindMemberPairs(unmappedDestinations, _sourceFields.Values);
            foreach (var pair in matchedPairs)
            {
                var expression = _waylessConfiguration.ExpressionBuilder.GetMapExpression<TSource>(pair.DestinationMember, pair.SourceMember);
                _fieldExpressions.Add(pair.DestinationMember.Name, expression);
            }
        }

        // get property name
        private static string GetMemberName<T>(Expression<Func<T, object>> expression)         
        {
            var propertyInfo = expression.GetMemberInfo();

            return propertyInfo.Name;
        }

        #endregion helpers
    }
}
