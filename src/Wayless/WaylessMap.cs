using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Wayless
{
    public sealed class WaylessMap<TDestination, TSource>
        : IWaylessMap<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        /// Type activator. Using static compiled expression for optimal performance
        private static readonly Func<TDestination> _destinationActivator = Expression.Lambda<Func<TDestination>>(Expression.New(typeof(TDestination)
                                                                              .GetConstructor(Type.EmptyTypes)))
                                                                              .Compile();


        private readonly IList<Map<TDestination, TSource>> _mappings = new List<Map<TDestination, TSource>>();

        private readonly IDictionary<string, (PropertyInfo, PropertyInfo)> _pairs;
        private readonly IDictionary<string, PropertyInfo> _destinationProperties;
        private readonly IDictionary<string, PropertyInfo> _sourceProperties;

        private Action<TDestination, TSource> _map;

        private bool _isCompiled;
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

            _pairs = new Dictionary<string, (PropertyInfo, PropertyInfo)>();
            _isCompiled = false;
            _destinationProperties = DestinationType.GetPropertyDictionary<TDestination>();
            _sourceProperties = SourceType.GetPropertyDictionary<TSource>();

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
            var destination = GetName(destinationExpression);

            Map<TDestination, TSource> map = _mappings.SingleOrDefault(x => x.DestinationProperty == destination);
            if (map != default(Map<TDestination, TSource>))
            {
                var index = _mappings.IndexOf(map);
                _mappings[index] = new Map<TDestination, TSource>(destination, MappingExpression.Build(destinationExpression, sourceExpression));
            }
            else
            {
                map = new Map<TDestination, TSource>(destination, MappingExpression.Build(destinationExpression, sourceExpression));
                _mappings.Add(map);
            }

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
            var destination = GetName(destinationExpression);

            Map<TDestination, TSource> map = _mappings.SingleOrDefault(x => x.DestinationProperty == destination);
            if (map != default(Map<TDestination, TSource>))
            {
                var index = _mappings.IndexOf(map);
                _mappings[index] = new Map<TDestination, TSource>(destination, MappingExpression.Build<TDestination, TSource>(destinationExpression, value));
            }
            else
            {
                map = new Map<TDestination, TSource>(destination, MappingExpression.Build<TDestination, TSource>(destinationExpression, value));
                _mappings.Add(map);
            }

            return this;
        }

        /// <summary>
        /// Create mapping rule for properties to be skipped in destination type
        /// </summary>
        /// <param name="ignoreAtDestinationExpression">Expression for property to be skipped in destination type</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWaylessMap<TDestination, TSource> FieldSkip(Expression<Func<TDestination, object>> ignoreAtDestinationExpression)
        {
            var ignore = GetName(ignoreAtDestinationExpression);

            Map<TDestination, TSource> map = _mappings.SingleOrDefault(x => x.DestinationProperty == ignore);
            if (map != default(Map<TDestination, TSource>))
            {
                _mappings.Remove(map);
            }

            return this;
        }

        #region helpers

        // apply all mapping rules
        private void InternalMap(TDestination destinationObject, TSource sourceObject)
        {
            if(!_isCompiled)
            {
                _map = MappingExpression.BuildMap<TDestination, TSource>(_pairs.Values);
                _isCompiled = true;
            }

            _map(destinationObject, sourceObject);
        }

        // create initial mapping dictionary by matching property (key) names
        private void GenerateDefaultMappingDictionary()
        {            
            foreach (var destinationInfo in _destinationProperties)
            {
                if (_sourceProperties.TryGetValue(destinationInfo.Key, out PropertyInfo sourceInfo))
                {
                    _pairs.Add(destinationInfo.Key.ToLowerInvariant(),(destinationInfo.Value, sourceInfo));
                }
            }

            _map = MappingExpression.BuildMap<TDestination, TSource>(_pairs.Values);
            _isCompiled = true;
        }

        // determine if key is properties true name or invariant lower case
        private static string GetName<T>(Expression<Func<T, object>> expression)
            where T : class
        {
            var propertyInfo = expression.GetMember<T, PropertyInfo>();

            return propertyInfo.Name;
        }

        #endregion helpers
    }
}
