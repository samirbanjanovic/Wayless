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
        private readonly IDictionary<string, IPropertyInfoPair> _mappingDictionary;

        private readonly IDictionary<string, IPropertyDetails> _destinationProperties;
        private readonly IDictionary<string, IPropertyDetails> _sourceProperties;

        private readonly bool _ignoreCasing;
        private readonly TSource _sourceObject;

        private IDictionary<string, object> _explicitAssignments;
        private IList<string> _skipFields;

        private bool _hasAppliedFieldSkipSet;

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

        public void Map(TDestination destinationObject, TSource sourceObject)
        {
            InternalMap(destinationObject, sourceObject);
        }

        public void Map(TDestination destinationObject)
        {
            InternalMap(destinationObject, _sourceObject);
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

        public IWaylessMap<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression)
        {
            var destinationKey = GetKey(destinationExpression);

            var sourceKey = GetKey(sourceExpression);

            if (_mappingDictionary.TryGetValue(destinationKey, out IPropertyInfoPair mappingPair))
            {
                if (_sourceProperties.TryGetValue(sourceKey, out IPropertyDetails fromSource))
                {
                    var mapping = _mappingDictionary[destinationKey];
                    mapping.SourceProperty = fromSource;
                }
            }
            else
            {
                if ((_destinationProperties.TryGetValue(destinationKey, out IPropertyDetails destinationDetails)) &&
                   (_sourceProperties.TryGetValue(sourceKey, out IPropertyDetails sourceDetails)))
                {
                    mappingPair = GetPropertyPair(destinationDetails, sourceDetails);
                }

                _mappingDictionary.Add(destinationKey, mappingPair);
            }

            return this;
        }

        public IWaylessMap<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpressoin, object fieldValue)
        {
            if (_explicitAssignments == null)
            {
                _explicitAssignments = new Dictionary<string, object>();
            }

            var destinationKey = GetKey(destinationExpressoin);

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

        public IEnumerable<string> ShowMapping()
        {
            return _mappingDictionary.Values.Select(v => $"{SourceType.Name}.{v.SourceProperty.Name} => {DestinationType.Name}.{v.DestinationProperty.Name}").ToList();
        }

        #region helpers

        private void InternalMap(TDestination destinationObject, TSource sourceObject)
        {
            ApplyFieldSkipToMappingDictionary();

            ApplyMappingDictionary(destinationObject, sourceObject);

            ApplyExplicitAssignments(destinationObject, sourceObject);
        }

        private void ApplyMappingDictionary(TDestination destinationObject, TSource sourceObject)
        {
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
            }
        }

        private void ApplyExplicitAssignments(TDestination destinationObject, TSource sourceObject)
        {
            if (_explicitAssignments != null)
            {
                foreach (var explicitAssignment in _explicitAssignments)
                {
                    if (_destinationProperties.TryGetValue(explicitAssignment.Key, out IPropertyDetails propertyDetails))
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

        private void SetValueWithConversion(IPropertyDetails destinationProperty, TDestination destinationObject, object value)
        {
            var converter = TypeDescriptor.GetConverter(value.GetType());
            if (converter.CanConvertTo(destinationProperty.PropertyInfo.PropertyType))
            {
                var convertedValue = converter.ConvertTo(value, destinationProperty.PropertyInfo.PropertyType);
                destinationProperty.PropertyInfo.SetValue(destinationObject, convertedValue);
            }
        }


        private string GetKey<T>(Expression<Func<T, object>> expression)
            where T : class
        {
            var propertyInfo = expression.GetMember<T, PropertyInfo>();
            var propertyName = propertyInfo.Name;

            return _ignoreCasing ? propertyName.ToLowerInvariant() : propertyName;
        }

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

        private IPropertyInfoPair GetPropertyPair(IPropertyDetails destinationDetails, IPropertyDetails sourceDetails)
        {
            var propertyInfoPair = new PropertyInfoPair
            {
                DestinationProperty = destinationDetails,
                SourceProperty = sourceDetails
            };

            return propertyInfoPair;
        }

        #endregion helpers
    }
}
