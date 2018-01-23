using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless
{
    internal class PropertyInfoPair<TDestination, TSource> 
    {
        public PropertyInfoPair(PropertyDetails<TDestination> destinationProperty, PropertyDetails<TSource> sourceProperty)
        {
            DestinationProperty = destinationProperty;
            SourceProperty = sourceProperty;
        }

        public PropertyDetails<TDestination> DestinationProperty { get; set; }
        
        public PropertyDetails<TSource> SourceProperty { get; set; }
        
        public object ValueToSet { get; }
    }
}
