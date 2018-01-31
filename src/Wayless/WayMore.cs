using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Wayless.Core;

namespace Wayless
{
    public sealed class WayMore
    {
        private static readonly Lazy<WayMore> _instance = new Lazy<WayMore>(() => new WayMore());

        private static readonly ConcurrentDictionary<object, object> _mappers = new ConcurrentDictionary<object, object>();

        public static WayMore Mappers
        {
            get
            {
                return _instance.Value;
            }
        }

        public IWayless<TDestination, TSource> Get<TDestination, TSource>()
        {
            return Get<TDestination, TSource>(WaylessConfigurationBuilder.GetDefaultConfiguration<TDestination, TSource>());
        }

        public IWayless<TDestination, TSource> Get<TDestination, TSource>(IWaylessConfiguration configuration)
        {
            var key = (typeof(TDestination), typeof(TSource), configuration.ExpressionBuilder?.GetType(), configuration.MatchMaker?.GetType()).GetHashCode();
            if (!_mappers.TryGetValue(key, out object mapper))
            {
                mapper = GetNew<TDestination, TSource>();
                _mappers.TryAdd(key, mapper);
            }

            return (IWayless<TDestination, TSource>)mapper;
        }

        public IWayless<TDestination, TSource> Get<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
        {
            return Get<TDestination, TSource>();
        }

        public IWayless<TDestination, TSource> Get<TDestination, TSource>(TDestination destinationObject, TSource sourceObject, IWaylessConfiguration configuration)
        {
            return Get<TDestination, TSource>(configuration);
        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>()
        {
            return GetNew<TDestination, TSource>(WaylessConfigurationBuilder.GetDefaultConfiguration<TDestination, TSource>());
        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>(IWaylessConfiguration configuration)
        {
            return new Wayless<TDestination, TSource>(configuration);            
        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
        {
            return GetNew<TDestination, TSource>();
        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>(TDestination destinationObject, TSource sourceObject, IWaylessConfiguration configuration)
        {
            return GetNew<TDestination, TSource>(configuration);
        }


    }
}