using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Wayless.Core;

namespace Wayless.PairMatching
{
    /// <summary>
    /// Create mapping using a simple Key:Value Json file
    /// Where Key = Destination property
    ///     Value = Source property
    /// </summary>
    public class JsonFileMatchMaker
        : IMatchMaker
    {
        private readonly string _path;

        public JsonFileMatchMaker(string path)
        {
            _path = path;
        }

        public IEnumerable<IMemberPair> FindMemberPairs(IEnumerable<MemberInfo> unassignedMembers, IEnumerable<MemberInfo> sourceMembers)
        {
            string jsonFile = File.ReadAllText(_path);

            IList<IMemberPair> memberPairs = new List<IMemberPair>();

            // create dictionary to look up source properties
            IDictionary<string, MemberInfo> sources = sourceMembers.ToDictionary(x => x.Name);

            // create dictionary to look up remaining desitnation properties
            IDictionary<string, MemberInfo> destinations = unassignedMembers.ToDictionary(x => x.Name);

            JsonSerializerSettings jsonSerializer = new JsonSerializerSettings()
            {
                Culture = CultureInfo.InvariantCulture
            };

            IDictionary<string, string> mappingDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonFile);

            foreach (var destination in destinations)
            {
                // if mapping file contains destination property 
                // find source property to use
                if (mappingDictionary.TryGetValue(destination.Value.Name, out string source))
                {
                    if (sources.TryGetValue(source, out MemberInfo sourceMember))
                    {
                        var pair = new MemberPair()
                        {
                            DestinationMember = destination.Value,
                            SourceMember = sourceMember
                        };

                        memberPairs.Add(pair);
                    }
                }
            }

            return memberPairs;
        }
    }
}
