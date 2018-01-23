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
        /// <summary>
        /// Stores mapping rules defining what value to apply to which destination property 
        /// Depending on _ignoreCasing value key is either the true property name or lower case invariant
        /// </summary>
        private readonly IDictionary<string, IPropertyInfoPair> _mappingDictionary;

        /// <summary>
        /// Dictionary of discovered destination type properties
        /// Depending on _ignoreCasing value key is either the true property name or lower case invariant
        /// </summary>
        private readonly IDictionary<string, IPropertyDetails> _destinationProperties;

        /// <summary>
        /// Dictionary of discovered source type properties
        /// /// Depending on _ignoreCasing value key is either the true property name or lower case invariant
        /// </summary>
        private readonly IDictionary<string, IPropertyDetails> _sourceProperties;

        private readonly bool _ignoreCasing;

        /// <summary>
        /// Properties to receive an explicit value  assignment
        /// object: value to assign
        /// </summary>
        private IDictionary<string, object> _explicitAssignments;

        /// <summary>
        /// Destination properties to be excluded from mapping rules
        /// </summary>
        private IList<string> _skipFields;

        private bool _hasAppliedFieldSkipSet;




        private readonly Func<TDestination> _destinationActivator = Expression.Lambda<Func<TDestination>>(Expression.New(typeof(TDestination)
                                                                              .GetConstructor(Type.EmptyTypes)))
                                                                              .Compile();    
        /// <summary>
        /// Create instance of Wayless mapper
        /// </summary>
        /// <param name="ignoreCasing">
        ///     True: Ignores casing during  initial property matchup
        ///     False: Property name casing affects matching
        /// </param>
        public WaylessMap(bool ignoreCasing = false)
        {
            _ignoreCasing = ignoreCasing;
            _hasAppliedFieldSkipSet = false;

            _mappingDictionary = new Dictionary<string, IPropertyInfoPair>();

            SourceType = typeof(TSource);
            DestinationType = typeof(TDestination);

            _destinationProperties = DestinationType.GetPropertyDictionary(_ignoreCasing);
            _sourceProperties = SourceType.GetPropertyDictionary(_ignoreCasing);

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
        public IEnumerable<TDestination> Map(IEnumerable<TSource> sourceList, params object[] constructorParameters)
        {
            foreach (var sourceObject in sourceList)
            {
                yield return Map(sourceObject, constructorParameters);
            }
        }

        /// <summary>
        /// Apply mapping rules in source object
        /// </summary>
        /// <param name="sourceObject">Object to read avalues from</param>
        /// <param name="constructorParameters">Parameters passed to destination object constructor</param>
        /// <returns>Mapped object</returns>
        public TDestination Map(TSource sourceObject, params object[] constructorParameters)
        {
            //var destinationObject = (TDestination)Activator.CreateInstance(DestinationType, constructorParameters);
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

            if (_mappingDictionary.TryGetValue(destinationKey, out IPropertyInfoPair mappingPair))
            {// replace existing mapping rule for property if one exists
                if (_sourceProperties.TryGetValue(sourceKey, out IPropertyDetails fromSource))
                {
                    var mapping = _mappingDictionary[destinationKey];
                    mapping.SourceProperty = fromSource;
                }
            }
            else
            {
                // add the new mapping rule to mapping dictionary
                if ((_destinationProperties.TryGetValue(destinationKey, out IPropertyDetails destinationDetails)) &&
                   (_sourceProperties.TryGetValue(sourceKey, out IPropertyDetails sourceDetails)))
                {
                    mappingPair = GetPropertyPair(destinationDetails, sourceDetails);
                }

                _mappingDictionary.Add(destinationKey, mappingPair);
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
            if (_skipFields == null)
            {
                _skipFields = new List<string>();
            }

            var destinationkey = GetKey(ignoreAtDestinationExpression);

            if (!_skipFields.Contains(destinationkey))
            {
                _skipFields.Add(destinationkey);
                _hasAppliedFieldSkipSet = false;
            }

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
            var skipAssignment = _skipFields?.Select(x => $"{DestinationType.Name}.{_destinationProperties[x].PropertyInfo.Name} - Skip").ToList();

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
            ApplyFieldSkipToMappingDictionary();

            ApplyMappingDictionary(destinationObject, sourceObject);

            ApplyExplicitAssignments(destinationObject, sourceObject);
        }

        // apply mapping dictionary
        private void ApplyMappingDictionary(TDestination destinationObject, TSource sourceObject)
        {
            foreach (var map in _mappingDictionary.Values)
            {

                //var value = map.Getter(sourceObject);
                //map.Setter(destinationObject, value);
                var sourceValue = map.SourceProperty.PropertyInfo.GetMethod.Invoke(sourceObject, null);
                map.DestinationProperty.PropertyInfo.SetValue(destinationObject, sourceValue);
            }
        }

        // apply explicit assignments
        private void ApplyExplicitAssignments(TDestination destinationObject, TSource sourceObject)
        {
            if (_explicitAssignments != null)
            {
                foreach (var explicitAssignment in _explicitAssignments)
                {
                    if (_destinationProperties.TryGetValue(explicitAssignment.Key, out IPropertyDetails propertyDetails))
                    {
                        if (explicitAssignment.Value.GetType() != propertyDetails.PropertyInfo.PropertyType)
                        {// rudementary logic  to check if we need to use a converter
                         // this can be improved since some types can be implicitly converted
                            SetValueWithConversion(propertyDetails, destinationObject, explicitAssignment.Value);
                        }
                        else
                        {
                            propertyDetails.PropertyInfo.SetValue(destinationObject, explicitAssignment.Value);
                        }
                    }
                }
            }
        }

        // remove rules from both mapping dictionary and explicit assignment
        private void ApplyFieldSkipToMappingDictionary()
        {
            if (_hasAppliedFieldSkipSet || _skipFields == null)
                return;

            foreach (var fieldKey in _skipFields)
            {
                if (_mappingDictionary.ContainsKey(fieldKey))
                {
                    _mappingDictionary.Remove(fieldKey);
                }

                if (_explicitAssignments?.ContainsKey(fieldKey) == true)
                {
                    _explicitAssignments.Remove(fieldKey);
                }
            }

            _hasAppliedFieldSkipSet = true;
        }

        // try applying value using basic conversion
        private void SetValueWithConversion(IPropertyDetails destinationProperty, TDestination destinationObject, object value)
        {
            var converter = TypeDescriptor.GetConverter(value.GetType());
            if (converter.CanConvertTo(destinationProperty.PropertyInfo.PropertyType))
            {
                var convertedValue = converter.ConvertTo(value, destinationProperty.PropertyInfo.PropertyType);
                destinationProperty.PropertyInfo.SetValue(destinationObject, convertedValue);
            }
        }

        // determine if key is properties true name or invariant lower case
        private string GetKey<T>(Expression<Func<T, object>> expression)
            where T : class
        {
            var propertyInfo = expression.GetMember<T, PropertyInfo>();
            var propertyName = propertyInfo.Name;

            return _ignoreCasing ? propertyName.ToLowerInvariant() : propertyName;
        }

        // create initial mapping dictionary by matching property (key) names
        private void GenerateDefaultMappingDictionary()
        {
            foreach (var destinationDetails in _destinationProperties)
            {
                if (_sourceProperties.TryGetValue(destinationDetails.Key, out IPropertyDetails sourceDetails))
                {
                    var mapping = GetPropertyPair(destinationDetails.Value, sourceDetails);
                    _mappingDictionary.Add(destinationDetails.Key, mapping);
                }
            }
        }

        // create mapping pair
        private IPropertyInfoPair GetPropertyPair(IPropertyDetails destinationDetails, IPropertyDetails sourceDetails)
        {
            var propertyInfoPair = new PropertyInfoPair
            {
                DestinationProperty = destinationDetails,
                SourceProperty = sourceDetails
            };

            

            //propertyInfoPair.Setter = (Action<object, object>)Delegate.CreateDelegate(typeof(Action<object>), null, propertyInfoPair.DestinationProperty.PropertyInfo.GetSetMethod());
            //propertyInfoPair.Getter = (Func<object, object>)Delegate.CreateDelegate(typeof(Func<object>), null, propertyInfoPair.SourceProperty.PropertyInfo.GetGetMethod());


            return propertyInfoPair;
        }

        #endregion helpers
    }
}
