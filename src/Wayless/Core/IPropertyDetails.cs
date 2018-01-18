using System.Reflection;

namespace Wayless
{
    public interface IPropertyDetails
    {
        string InvarientName { get; }
        string Name { get; }
        PropertyInfo PropertyInfo { get; }
    }
}