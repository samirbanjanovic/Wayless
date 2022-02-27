using System.Reflection;

namespace Wayless.Core
{
    public interface IMemberPair
    {
        MemberInfo DestinationMember { get; set; }

        MemberInfo SourceMember { get; set; }
    }
}
