using System;
using System.Collections.Generic;
using System.Text;

namespace Wayless
{
    public class WaylessGeneric<TDestination, TSource>
        : Wayless
        where TDestination :class
        where TSource : class
    {
        public WaylessGeneric()
            : base(typeof(TDestination), typeof(TSource))
        { }
    }
}
