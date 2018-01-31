using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wayless.Core
{
    public interface IMatchMaker
    {
        IEnumerable<IMemberPair> FindMemberPairs(IEnumerable<MemberInfo> unassignedMembers, IEnumerable<MemberInfo> sourceMembers);           
    }
}
