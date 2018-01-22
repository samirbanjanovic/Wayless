using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wayless
{
    public class PropertyInfoPair 
        : IPropertyInfoPair
    {
        public IPropertyDetails DestinationProperty { get; set; }

        public IPropertyDetails SourceProperty { get; set; }

        public Action<object, object> Setter { get; set; }

        public Func<object, object> Getter { get; set; }
    }
}
