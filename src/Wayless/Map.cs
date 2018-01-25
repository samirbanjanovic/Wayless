using System;
using System.Collections.Generic;
using System.Text;

namespace Wayless
{
    internal sealed class Map<TDestiation, TSource>
    {
        public Map(string destinationProperty, Action<TDestiation, TSource> mapValue)
        {
            DestinationProperty = destinationProperty;            
            MapValue = mapValue;
        }

        public string DestinationProperty { get; }

        public string SourceProperty { get; }

        public Action<TDestiation, TSource> MapValue { get; }
    }
}
