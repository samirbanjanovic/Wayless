using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Wayless.ExpressionBuilders;

namespace Wayless
{
    public sealed class WaylessMap<TDestination, TSource>
        : IWaylessMap<TDestination, TSource>
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
        private static readonly ExpressionMapBuilder<TDestination, TSource> _expressionBuilder = new ExpressionMapBuilder<TDestination, TSource>();

        private readonly IDictionary<string, PropertyInfo> _destinationProperties;
        private readonly IDictionary<string, PropertyInfo> _sourceProperties;
        private readonly IDictionary<string, Expression> _fieldExpressions;

        // Indicates if the _compiledMap action has the latest rules
        // each time mapping is modified this flag will be switched
        // to false, letting Wayless know to compile a new 
        // mapping function from it's collected expressions
        private bool _isMapUpToDate;

        private Action<TDestination, TSource> _map;
                
        /// <summary>
        /// Create instance of Wayless mapper
        /// </summary>
        /// <param name="ignoreCasing">
        ///     True: Ignores casing during  initial property matchup
        ///     False: Property name casing affects matching
        /// </param>
        public WaylessMap()
        {
            SourceType = typeof(TSource);
            DestinationType = typeof(TDestination);

            _isMapUpToDate = false;

            _fieldExpressions = new Dictionary<string, Expression>();
            
            _destinationProperties = DestinationType.GetInvariantPropertyDictionary<TDestination>();
            _sourceProperties = SourceType.GetInvariantPropertyDictionary<TSource>();

            GenerateDefaultMappingDictionary();
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
        public IWaylessMap<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression)
        {
            var destination = GetInvariantName(destinationExpression);
            var expression = _expressionBuilder.GetPropertyFieldMapExpression(destinationExpression, sourceExpression);

            RegisterFieldExpression(destination, expression);

            _isMapUpToDate = false;
            return this;
        }

        /// <summary>
        /// Create mapping rule to explicitly assign value to destination property
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="value">Value to assign</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWaylessMap<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object value)
        {
            var destination = GetInvariantName(destinationExpression);
            var expression = _expressionBuilder.GetPropertyFieldSetExression(destinationExpression, value);

            RegisterFieldExpression(destination, expression);

            _isMapUpToDate = false;
            return this;
        }

        /// <summary>
        /// Create mapping rule for properties to be skipped in destination type
        /// </summary>
        /// <param name="ignoreAtDestinationExpression">Expression for property to be skipped in destination type</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWaylessMap<TDestination, TSource> FieldSkip(Expression<Func<TDestination, object>> ignoreAtDestinationExpression)
        {
            var ignore = GetInvariantName(ignoreAtDestinationExpression);
            if(_fieldExpressions.ContainsKey(ignore))
            {
                _fieldExpressions.Remove(ignore);
            }

            _isMapUpToDate = false;
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
            if(!_isMapUpToDate)
            {
                _map = _expressionBuilder.BuildActionMap(_fieldExpressions.Values);
                _isMapUpToDate = true;
            }

            _map(destinationObject, sourceObject);
        }

        // create initial mapping dictionary by matching property (key) names
        private void GenerateDefaultMappingDictionary()
        {            
            foreach(var destination in _destinationProperties)
            {
                if(_sourceProperties.TryGetValue(destination.Key, out PropertyInfo source))
                {
                    var expression = _expressionBuilder.GetPropertyFieldMapExpression(destination.Value, source);
                    _fieldExpressions.Add(destination.Key, expression);
                }
            }
        }

        // get property name
        private static string GetInvariantName<T>(Expression<Func<T, object>> expression)
            where T : class
        {
            var propertyInfo = expression.GetPropertyInfo<T>();

            return propertyInfo.Name.ToLowerInvariant();
        }

        #endregion helpers
    }
}
