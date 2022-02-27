using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Wayless.Core;

namespace Wayless
{
    public class MemberPair
        : IMemberPair
    {
        public MemberInfo DestinationMember { get; set; }
        public MemberInfo SourceMember { get; set; }
    }
}
