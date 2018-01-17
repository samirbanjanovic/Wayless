using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless
{
    public class WaylessMap<TSource, TDestination>
     where TSource : class
     where TDestination : class
    {
        private readonly IDictionary<string, PropertyInfoPair> _mappingDictionary;

        private readonly IDictionary<string, PropertyDetails> _destinationProperties;
        private readonly IDictionary<string, PropertyDetails> _sourceProperties;

        private readonly bool _ignoreCasing;
        private readonly TSource _sourceObject;

        private IDictionary<string, object> _explicitAssignments;
        private IList<string> _skipFields;

        private bool _hasAppliedFieldSkipSet;

        public WaylessMap(bool ignoreCasing = false)
        {
            _ignoreCasing = ignoreCasing;
            _hasAppliedFieldSkipSet = false;

            _mappingDictionary = new Dictionary<string, PropertyInfoPair>();

            SourceType = typeof(TSource);
            DestinationType = typeof(TDestination);

            _destinationProperties = DestinationType.GetPropertyDictionary(_ignoreCasing);
            _sourceProperties = SourceType.GetPropertyDictionary(_ignoreCasing);

            GenerateDefaultMappingDictionary();
        }

        public WaylessMap(TSource sourceObject, bool ignoreCasing = false)
            : this(ignoreCasing)
        {
            _sourceObject = sourceObject;
        }

        public Type SourceType { get; }

        public Type DestinationType { get; }

        public void Map(ref TDestination destinationObject, ref TSource sourceObject)
        {
            InternalMap(destinationObject, sourceObject);
        }

        public IEnumerable<TDestination> Map(IEnumerable<TSource> sourceList, params object[] constructorParameters)
        {
            foreach (var sourceObject in sourceList)
            {
                yield return Map(sourceObject, constructorParameters);
            }
        }

        public TDestination Map(params object[] constructorParameters)
        {
            var destinationObject = (TDestination)Activator.CreateInstance(DestinationType, constructorParameters);
            InternalMap(destinationObject, _sourceObject);

            return destinationObject;
        }

        public TDestination Map(TSource sourceObject, params object[] constructorParameters)
        {
            var destinationObject = (TDestination)Activator.CreateInstance(DestinationType, constructorParameters);

            InternalMap(destinationObject, sourceObject);

            return destinationObject;
        }

        public WaylessMap<TSource, TDestination> FieldMap(Expression<Func<TDestination, object>> destination, Expression<Func<TSource, object>> source)
        {
            var destinationKey = GetKey(destination.GetMember<TDestination, PropertyInfo>());
            var sourceKey = GetKey(source.GetMember<TSource, PropertyInfo>());

            if (_mappingDictionary.TryGetValue(destinationKey, out PropertyInfoPair mappingPair))
            {
                if (_sourceProperties.TryGetValue(sourceKey, out PropertyDetails fromSource))
                {
                    var mapping = _mappingDictionary[destinationKey];
                    mapping.PropertyPair.SourcePropertyName = fromSource.Name;
                    mapping.SourceProperty = fromSource;
                }
            }
            else
            {
                if ((_destinationProperties.TryGetValue(destinationKey, out PropertyDetails destinationDetails)) &&
                   (_sourceProperties.TryGetValue(sourceKey, out PropertyDetails sourceDetails)))
                {
                    mappingPair = GetPropertyPair(destinationDetails, sourceDetails);
                }

                _mappingDictionary.Add(destinationKey, mappingPair);
            }

            return this;
        }

        public WaylessMap<TSource, TDestination> FieldSet(Expression<Func<TDestination, object>> destination, object fieldValue)
        {
            if (_explicitAssignments == null)
            {
                _explicitAssignments = new Dictionary<string, object>();
            }
            var destinationKey = GetKey(destination.GetMember<TDestination, PropertyInfo>());

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

        public WaylessMap<TSource, TDestination> SkipAssignment(Expression<Func<TDestination, object>> ignoreAtDestination)
        {
            if (_skipFields == null)
            {
                _skipFields = new List<string>();
            }

            var destinationkey = GetKey(ignoreAtDestination.GetMember<TDestination, PropertyInfo>());

            if (!_skipFields.Contains(destinationkey))
            {
                _skipFields.Add(destinationkey);
                _hasAppliedFieldSkipSet = false;
            }

            return this;
        }

        public IEnumerable<PropertyPair> ShowMapping()
        {
            return _mappingDictionary.Values.Select(v => v.PropertyPair).ToList();
        }

        #region helpers

        private void InternalMap(TDestination destinationObject, TSource sourceObject)
        {
            SetSkipAssignmentInMappingDictionary();

            if (_mappingDictionary.Values.Count > 0)
            {
                foreach (var map in _mappingDictionary.Values)
                {
                    var sourceValue = map.SourceProperty.PropertyInfo.GetValue(sourceObject);

                    if (map.SourceProperty.PropertyInfo.PropertyType != map.DestinationProperty.PropertyInfo.PropertyType)
                    {// perform some basic conversion

                        SetValueWithConversion(map.DestinationProperty, destinationObject, sourceValue);
                    }
                    else
                    {
                        map.DestinationProperty.PropertyInfo.SetValue(destinationObject, sourceValue);
                    }
                }

                if (_explicitAssignments != null)
                {
                    foreach (var explicitAssignment in _explicitAssignments)
                    {
                        if (_destinationProperties.TryGetValue(explicitAssignment.Key, out PropertyDetails propertyDetails))
                        {
                            if (explicitAssignment.Value.GetType() != propertyDetails.PropertyInfo.PropertyType)
                            {
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
        }

        private void SetValueWithConversion(PropertyDetails destinationProperty, TDestination destinationObject, object value)
        {
            var converter = TypeDescriptor.GetConverter(value.GetType());
            if (converter.CanConvertTo(destinationProperty.PropertyInfo.PropertyType))
            {
                var convertedValue = converter.ConvertTo(value, destinationProperty.PropertyInfo.PropertyType);
                destinationProperty.PropertyInfo.SetValue(destinationObject, convertedValue);
            }
        }


        private void SetSkipAssignmentInMappingDictionary()
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

        private string GetKey(PropertyInfo propertyInfo)
        {
            var propertyName = propertyInfo.Name;

            return _ignoreCasing ? propertyName.ToLowerInvariant() : propertyName;
        }

        private void GenerateDefaultMappingDictionary()
        {
            foreach (var destinationDetails in _destinationProperties)
            {
                if (_sourceProperties.TryGetValue(destinationDetails.Key, out PropertyDetails sourceDetails))
                {
                    var mapping = GetPropertyPair(destinationDetails.Value, sourceDetails);
                    _mappingDictionary.Add(destinationDetails.Key, mapping);
                }
            }
        }

        private PropertyInfoPair GetPropertyPair(PropertyDetails destinationDetails, PropertyDetails sourceDetails)
        {
            var propertyPair = new PropertyInfoPair
            {
                PropertyPair = new PropertyPair
                {
                    SourcePropertyName = sourceDetails.PropertyInfo.Name,
                    DestinationPropertyName = destinationDetails.PropertyInfo.Name
                },
                DestinationProperty = destinationDetails,
                SourceProperty = sourceDetails
            };

            return propertyPair;
        }

        #endregion helpers
    }
}
