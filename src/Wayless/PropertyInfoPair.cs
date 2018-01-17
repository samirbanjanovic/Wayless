using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wayless
{
    internal class PropertyInfoPair
    {
        public PropertyPair PropertyPair { get; set; }

        public PropertyDetails DestinationProperty { get; set; }

        public PropertyDetails SourceProperty { get; set; }
    }
}
