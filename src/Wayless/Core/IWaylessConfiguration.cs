using System;
using System.Collections.Generic;
using System.Text;

namespace Wayless.Core
{
    public interface IWaylessConfiguration
    {
        bool AutoMatchMembers { get; set; }

        IExpressionBuilder ExpressionBuilder { get; set; }

        IMatchMaker MatchMaker { get; set; }
    }
}
