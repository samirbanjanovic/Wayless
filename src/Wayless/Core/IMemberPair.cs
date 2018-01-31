using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wayless.Core
{
    public interface IMemberPair
    {
        MemberInfo DestinationMember { get; set; }

        MemberInfo SourceMember { get; set; }
    }
}
