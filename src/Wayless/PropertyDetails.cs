using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wayless
{
    public class PropertyDetails 
        : IPropertyDetails
    {
        public PropertyDetails(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            Name = PropertyInfo.Name;
            InvarientName = Name.ToLowerInvariant();
        }

        public string Name { get; }

        public string InvarientName { get; }

        public PropertyInfo PropertyInfo { get; }
    }
}
