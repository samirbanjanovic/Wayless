using System;
using System.Collections.Generic;
using Wayless.Core;

namespace Wayless.Core
{
    public interface IWayMore
    {
        IWayMore SetRules<TDestination, TSource>(Action<ISetRuleBuilder<TDestination, TSource>> mapperRules)
            where TDestination : class
            where TSource : class;

        void Map<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class;
        TDestination Map<TDestination, TSource>(TSource sourceObject)
            where TDestination : class
            where TSource : class;
        IEnumerable<TDestination> Map<TDestination, TSource>(IEnumerable<TSource> sourceObject)
            where TDestination : class
            where TSource : class;
        
        IWayless<TDestination, TSource> Get<TDestination, TSource>()
            where TDestination : class
            where TSource : class;
        IWayless<TDestination, TSource> Get<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class;
    }
}