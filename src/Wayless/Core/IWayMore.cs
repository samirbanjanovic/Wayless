using System;
using System.Collections.Generic;
using Wayless.Core;

namespace Wayless.Core
{
    public interface IWayMore
    {
        IWayMore SetRules<TDestination, TSource>(Action<ISetRuleBuilder<TDestination, TSource>> config)
            where TDestination : class
            where TSource : class;

        IWayMore SetRules<TDestination, TSource>(IWaylessConfiguration configuration, Action<ISetRuleBuilder<TDestination, TSource>> config)
            where TDestination : class
            where TSource : class;

        IEnumerable<TDestination> Map<TDestination, TSource>(IEnumerable<TSource> sourceObject)
            where TDestination : class
            where TSource : class;
        IEnumerable<TDestination> Map<TDestination, TSource>(IEnumerable<TSource> sourceObject, IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class;
        void Map<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class;
        void Map<TDestination, TSource>(TDestination destinationObject, TSource sourceObject, IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class;
        TDestination Map<TDestination, TSource>(TSource sourceObject)
            where TDestination : class
            where TSource : class;
        TDestination Map<TDestination, TSource>(TSource sourceObject, IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class;

        IWayless<TDestination, TSource> Get<TDestination, TSource>()
            where TDestination : class
            where TSource : class;
        IWayless<TDestination, TSource> Get<TDestination, TSource>(IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class;
        IWayless<TDestination, TSource> Get<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class;
        IWayless<TDestination, TSource> Get<TDestination, TSource>(TDestination destinationObject, TSource sourceObject, IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class;

        IWayless<TDestination, TSource> GetNew<TDestination, TSource>()
            where TDestination : class
            where TSource : class;
        IWayless<TDestination, TSource> GetNew<TDestination, TSource>(IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class;
        IWayless<TDestination, TSource> GetNew<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class;
        IWayless<TDestination, TSource> GetNew<TDestination, TSource>(TDestination destinationObject, TSource sourceObject, IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class;


    }
}