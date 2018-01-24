using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless
{
    internal class PropertyDetails<T>         
    {
        public PropertyDetails(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            Name = PropertyInfo.Name;            
        }

        public string Name { get; }

        public PropertyInfo PropertyInfo { get; }
    }
}
