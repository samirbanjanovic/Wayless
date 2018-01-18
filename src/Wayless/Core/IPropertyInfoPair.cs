namespace Wayless
{
    public interface IPropertyInfoPair
    {
        IPropertyDetails DestinationProperty { get; set; }
        IPropertyDetails SourceProperty { get; set; }
    }
}