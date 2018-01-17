using System;
using System.Collections.Generic;
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

        private readonly IList<Action<TDestination>> _explicitDirectAssignments;
        private readonly IList<Action<TSource, TDestination>> _explicitMappingAssigments;

        private readonly bool _ignoreCasing;
        private readonly TSource _sourceObject;

        public WaylessMap(bool ignoreCasing = false)
        {
            _ignoreCasing = ignoreCasing;
            _mappingDictionary = new Dictionary<string, PropertyInfoPair>();
            _explicitDirectAssignments = new List<Action<TDestination>>();
            _explicitMappingAssigments = new List<Action<TSource, TDestination>>();

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

        public void Map(ref TSource sourceObject, ref TDestination destinationObject)
        {
            InternalMap(sourceObject, destinationObject);
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
            InternalMap(_sourceObject, destinationObject);

            return destinationObject;
        }

        public TDestination Map(TSource sourceObject, params object[] constructorParameters)
        {
            var destinationObject = (TDestination)Activator.CreateInstance(DestinationType, constructorParameters);

            InternalMap(sourceObject, destinationObject);

            return destinationObject;
        }

        private void InternalMap(TSource sourceObject, TDestination destinationObject)
        {
            MapFromDictionary(sourceObject, destinationObject);
            ExecuteExplicitDirectAssignment(sourceObject, destinationObject);
            ExecuteExplicitMappingAssignments(sourceObject, destinationObject);
        }

        private void MapFromDictionary(TSource sourceObject, TDestination destinationObject)
        {
            if(_mappingDictionary.Values.Count > 0)
            {
                foreach (var map in _mappingDictionary.Values)
                {
                    var sourceValue = map.SourceProperty.PropertyInfo.GetValue(sourceObject);
                    map.DestinationProperty.PropertyInfo.SetValue(destinationObject, sourceValue);
                }
            }            
        }

        private void ExecuteExplicitDirectAssignment(TSource sourceObject, TDestination destinationObject)
        {
            if (_explicitDirectAssignments.Count > 0)
            {
                foreach (var explicitAssign in _explicitDirectAssignments)
                {
                    explicitAssign(destinationObject);
                }
            }
        }

        private void ExecuteExplicitMappingAssignments(TSource sourceObject, TDestination destinationObject)
        {
            if (_explicitMappingAssigments.Count > 0)
            {
                foreach (var explicitMapAssign in _explicitMappingAssigments)
                {
                    explicitMapAssign(sourceObject, destinationObject);
                }
            }
        }

        public WaylessMap<TSource, TDestination> Explicit(Expression<Func<TSource, object>> source, Expression<Func<TDestination, object>> destination)
        {
            var destinationKey = GetKey(destination.GetMember<TDestination, PropertyInfo>());
            var sourceKey = GetKey(source.GetMember<TSource, PropertyInfo>());

            if (_mappingDictionary.TryGetValue(destinationKey, out PropertyInfoPair mappingPair))
            {
                if (_sourceProperties.TryGetValue(sourceKey, out PropertyDetails fromSource))
                {
                    var mapping = _mappingDictionary[sourceKey];
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

        public WaylessMap<TSource, TDestination> Explicit(Action<TDestination> explicitAssignment)
        {
            if (explicitAssignment != null)
            {
                _explicitDirectAssignments.Add(explicitAssignment);
            }

            return this;
        }

        public WaylessMap<TSource, TDestination> Explicit(Action<TSource, TDestination> explicitAssignment)
        {
            if (explicitAssignment != null)
            {
                _explicitMappingAssigments.Add(explicitAssignment);
            }

            return this;
        }

        public WaylessMap<TSource, TDestination> Ignore(Expression<Func<TDestination, object>> ignoreAtDestination)
        {
            var destinationkey = GetKey(ignoreAtDestination.GetMember<TDestination, PropertyInfo>());

            if (_mappingDictionary.ContainsKey(destinationkey))
            {
                _mappingDictionary.Remove(destinationkey);
            }

            return this;
        }

        public IEnumerable<PropertyPair> ShowMapping()
        {
            return _mappingDictionary.Values.Select(v => v.PropertyPair).ToList();
        }

        #region helpers

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
