using System;

namespace Wayless
{
    public interface IPropertyInfoPair        
    {
        IPropertyDetails DestinationProperty { get; set; }
        IPropertyDetails SourceProperty { get; set; }

        Action<object, object> Setter { get; set; }

        Func<object, object> Getter { get; set; }
    }
}