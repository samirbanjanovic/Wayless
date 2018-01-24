﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Wayless
{
    public class WaylessMap<TDestination, TSource>
        : IWaylessMap<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        /// Type activator. Using static compiled expression for optimal performance
        private static readonly Func<TDestination> _destinationActivator = Expression.Lambda<Func<TDestination>>(Expression.New(typeof(TDestination)
                                                                              .GetConstructor(Type.EmptyTypes)))
                                                                              .Compile();

        /// <summary>
        /// Stores mapping rules defining what value to apply to which destination property 
        /// Depending on _ignoreCasing value key is either the true property name or lower case invariant
        /// </summary>
        //private readonly IDictionary<string, PropertyInfoPair<TDestination, TSource>> _defaultPairs;

        private readonly IDictionary<string, Action<TDestination, TSource>> _mappingDictionary;

        /// <summary>
        /// Destination properties to be excluded from mapping rules
        /// </summary>
        private IList<string> _fields;

        /// <summary>
        /// Create instance of Wayless mapper
        /// </summary>
        /// <param name="ignoreCasing">
        ///     True: Ignores casing during  initial property matchup
        ///     False: Property name casing affects matching
        /// </param>
        public WaylessMap()
        {            
            _mappingDictionary = new Dictionary<string, Action<TDestination, TSource>>();
            _fields = new List<string>();

            SourceType = typeof(TSource);
            DestinationType = typeof(TDestination);

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

        private readonly IList<Action<TDestination, TSource>> _mappingActions = new List<Action<TDestination, TSource>>();
        /// <summary>
        /// Create a mapping rule between destination property and 
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="sourceExpression">Expression for property to be read in source type</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWaylessMap<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression)
        {
            var destination = GetName(destinationExpression);
            
            if (_mappingDictionary.ContainsKey(destination))
            {
                _mappingDictionary[destination] = MappingExpression.Build(destinationExpression, sourceExpression);
            }
            else
            {
                _mappingDictionary.Add(destination, MappingExpression.Build(destinationExpression, sourceExpression));
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

            if (_mappingDictionary.ContainsKey(destination))
            {
                _mappingDictionary[destination] = MappingExpression.Build<TDestination, TSource>(destinationExpression, value);
            }
            else
            {
                _mappingDictionary.Add(destination, MappingExpression.Build<TDestination, TSource>(destinationExpression, value));
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
            var ignoreItem = GetName(ignoreAtDestinationExpression);
            if (_fields.Contains(ignoreItem))
            {
                _fields.Remove(ignoreItem);
            }

            return this;
        }

        /// <summary>
        /// Restores field to mapping dictionary if a prior instruction had it set to be skipped
        /// </summary>
        /// <param name="ignoredFieldToRestore">Field to restore</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWaylessMap<TDestination, TSource> FieldRestore(Expression<Func<TDestination, object>> ignoredFieldToRestore)
        {
            var restoreItem = GetName(ignoredFieldToRestore);
            if (!_fields.Contains(restoreItem))
            {
                _fields.Add(restoreItem);
            }

            return this;
        }


        #region helpers

        // apply all mapping rules
        private void InternalMap(TDestination destinationObject, TSource sourceObject)
        {
            foreach(var field in _fields)
            {
                _mappingDictionary[field](destinationObject, sourceObject);
            }            
        }

        // create initial mapping dictionary by matching property (key) names
        private void GenerateDefaultMappingDictionary()
        {
            var destinationProperties = DestinationType.GetPropertyDictionary<TDestination>();
            var sourceProperties = SourceType.GetPropertyDictionary<TSource>();

            foreach(var destinationInfo in destinationProperties)
            {
                _fields.Add(destinationInfo.Key);
                if (sourceProperties.TryGetValue(destinationInfo.Key, out PropertyInfo sourceInfo))
                {
                    _mappingDictionary.Add(destinationInfo.Key, MappingExpression.Build<TDestination, TSource>(destinationInfo.Value, sourceInfo));
                }                
            }
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
