using System;
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
        private static readonly Func<TDestination> _destinationActivator = Expression.Lambda<Func<TDestination>>(Expression.New(typeof(TDestination)
                                                                              .GetConstructor(Type.EmptyTypes)))
                                                                              .Compile();

        /// <summary>
        /// Stores mapping rules defining what value to apply to which destination property 
        /// Depending on _ignoreCasing value key is either the true property name or lower case invariant
        /// </summary>
        private readonly IDictionary<string, PropertyInfoPair<TDestination, TSource>> _mappingDictionary;

        /// <summary>
        /// Dictionary of discovered destination type properties
        /// Depending on _ignoreCasing value key is either the true property name or lower case invariant
        /// </summary>
        private readonly IDictionary<string, PropertyDetails<TDestination>> _destinationProperties;

        /// <summary>
        /// Dictionary of discovered source type properties
        /// /// Depending on _ignoreCasing value key is either the true property name or lower case invariant
        /// </summary>
        private readonly IDictionary<string, PropertyDetails<TSource>> _sourceProperties;

        /// <summary>
        /// Properties to receive an explicit value  assignment
        /// object: value to assign
        /// </summary>
        private IDictionary<string, object> _explicitAssignments;

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
            _mappingDictionary = new Dictionary<string, PropertyInfoPair<TDestination, TSource>>();
            _fields = new List<string>();
            SourceType = typeof(TSource);
            DestinationType = typeof(TDestination);

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
        public void Map(ref TDestination destinationObject, ref TSource sourceObject)
        {
            InternalMap(destinationObject, sourceObject);
        }

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
            var destinationKey = GetKey(destinationExpression);
            var sourceKey = GetKey(sourceExpression);

            if (_mappingDictionary.TryGetValue(destinationKey, out PropertyInfoPair<TDestination,TSource> mapping))
            {
                mapping.SourceProperty = _sourceProperties[sourceKey];
            }
            else
            {
                var destinationDetails = _destinationProperties[destinationKey];
                var sourceDetails = _sourceProperties[sourceKey];

                AddToMappingDictionary(destinationDetails, sourceDetails);              
            }

            return this;
        }

        /// <summary>
        /// Create mapping rule to explicitly assign value to destination property
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="fieldValue">Value to assign</param>
        /// <returns>Current instance of WaylessMap</returns>
        public IWaylessMap<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object fieldValue)
        {
            // delay instantiation of dictionary untill rule is required
            if (_explicitAssignments == null)
            {
                _explicitAssignments = new Dictionary<string, object>();
            }

            var destinationKey = GetKey(destinationExpression);

            // store assignment for evaluation at call to Map()
            if (_explicitAssignments.ContainsKey(destinationKey))
            {
                _explicitAssignments[destinationKey] = fieldValue;
            }
            else
            {
                _explicitAssignments.Add(destinationKey, fieldValue);
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
            _fields.Remove(GetKey(ignoreAtDestinationExpression));

            return this;
        }

        /// <summary>
        /// Returns readable mapping rules set in mapper
        /// </summary>
        /// <returns></returns>
        public string ShowMapping()
        {
            var mappingDictionary = _mappingDictionary.Values.Select(v => $"{DestinationType.Name}.{v.DestinationProperty.Name} = {SourceType.Name}.{v.SourceProperty.Name} ").ToList();
            var explicitAssignments = _explicitAssignments?.Select(x => $"{DestinationType.Name}.{_destinationProperties[x.Key].PropertyInfo.Name} = {x.Value.ToString()}").ToList();
            var skipAssignment = _fields?.Select(x => $"{DestinationType.Name}.{_destinationProperties[x].PropertyInfo.Name} - Skip").ToList();

            List<string> mappingRules = new List<string>();
            mappingRules.AddRange(mappingDictionary);

            if (explicitAssignments != null)
                mappingRules.AddRange(explicitAssignments);

            if (skipAssignment != null)
                mappingRules.AddRange(skipAssignment);

            return string.Join(Environment.NewLine, mappingRules);
        }

        #region helpers

        // apply all mapping rules
        private void InternalMap(TDestination destinationObject, TSource sourceObject)
        {
            ApplyMappingDictionary(destinationObject, sourceObject);

            ApplyExplicitAssignments(destinationObject, sourceObject);
        }

        private static readonly IDictionary<Type, TypeConverter> _typeConverters = new Dictionary<Type, TypeConverter>();
        // apply mapping dictionary
        private void ApplyMappingDictionary(TDestination destinationObject, TSource sourceObject)
        {
            foreach (var field in _fields)
            {
                var propertyPair = _mappingDictionary[field];
                object sourceValue = propertyPair.SourceProperty.ValueGet(sourceObject);
                propertyPair.DestinationProperty.ValueSet(destinationObject, sourceValue);
            }
        }

        // apply explicit assignments
        private void ApplyExplicitAssignments(TDestination destinationObject, TSource sourceObject)
        {
            if (_explicitAssignments != null)
            {
                foreach (var explicitAssignment in _explicitAssignments)
                {
                    PropertyDetails<TDestination> propertyDetails = _destinationProperties[explicitAssignment.Key];                    
                    propertyDetails.ValueSet(destinationObject, explicitAssignment.Value);
                }
            }
        }

        // create initial mapping dictionary by matching property (key) names
        private void GenerateDefaultMappingDictionary()
        {
            foreach (var destinationDetails in _destinationProperties)
            {
                _fields.Add(destinationDetails.Key);
                if (_sourceProperties.TryGetValue(destinationDetails.Key, out PropertyDetails<TSource> sourceDetails))
                {
                    AddToMappingDictionary(destinationDetails.Value, sourceDetails);
                }
            }
        }

        private void AddToMappingDictionary(PropertyDetails<TDestination> destinationDetails, PropertyDetails<TSource> sourceDetails)
        {
            var mapping = new PropertyInfoPair<TDestination, TSource>(destinationDetails, sourceDetails);
            _mappingDictionary.Add(destinationDetails.Name, mapping);
        }

        // determine if key is properties true name or invariant lower case
        private static string GetKey<T>(Expression<Func<T, object>> expression)
            where T : class
        {
            var propertyInfo = expression.GetMember<T, PropertyInfo>();

            return propertyInfo.Name;
        }

        #endregion helpers
    }
}
