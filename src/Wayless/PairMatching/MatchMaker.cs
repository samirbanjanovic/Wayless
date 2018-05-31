using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Wayless.Core;


namespace Wayless
{
    public class MatchMaker
        : IMatchMaker
    {
        public IEnumerable<IMemberPair> FindMemberPairs(IEnumerable<MemberInfo> unassignedMembers, IEnumerable<MemberInfo> sourceMembers)
        {
            // convert all property names to lower case (culture invariant)
            // match by ignoring case
            var destinationDictionary = ConvertToInvariantNameDictionary(unassignedMembers);
            var sourceDictionary = ConvertToInvariantNameDictionary(sourceMembers);

            IList<IMemberPair> memberPairs = new List<IMemberPair>();

            foreach(var destination in destinationDictionary)
            {
            // check if destination property has a matching source via name matching (case insensitive)
                if(sourceDictionary.TryGetValue(destination.Key, out MemberInfo source))
                {
                    IMemberPair pair = new MemberPair
                    {
                        DestinationMember = destination.Value,
                        SourceMember = source
                    };
                    memberPairs.Add(pair);
                }
            }

            return memberPairs;
        }

        private IDictionary<string, MemberInfo> ConvertToInvariantNameDictionary(IEnumerable<MemberInfo> members)
        {
            return members.ToDictionary(x => x.Name.ToLowerInvariant());
        }

    }
}
