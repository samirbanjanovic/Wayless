using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wayless
{
    public class PropertyDetails
    {
        public PropertyDetails(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            Name = PropertyInfo.Name;
            InvarientName = Name.ToLowerInvariant();
        }

        public string Name { get; }

        internal string InvarientName { get; }

        internal PropertyInfo PropertyInfo { get; }

    }
}
