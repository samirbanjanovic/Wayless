using System;
using System.Collections.Generic;
using System.Text;
using Wayless.Core;

namespace Wayless
{
    public class WaylessConfiguration
        : IWaylessConfiguration
    {
        public bool DontAutoMatchMembers { get; set; }
        public IExpressionBuilder ExpressionBuilder { get; set; }
        public IMatchMaker MatchMaker { get; set; }
    }
}
