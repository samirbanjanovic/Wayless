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
            where TDestination : class
            where TSource : class
        {
            return Get<TDestination, TSource>(WaylessConfigurationBuilder.GetDefaultConfiguration<TDestination, TSource>());
        }

        public IWayless<TDestination, TSource> Get<TDestination, TSource>(IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class
        {
            var key = (typeof(TDestination), typeof(TSource), configuration.ExpressionBuilder.GetType(), configuration.MatchMaker.GetType()).GetHashCode();
            if (!_mappers.TryGetValue(key, out object mapper))
            {
                mapper = GetNew<TDestination, TSource>();
                _mappers.TryAdd(key, mapper);
            }

            return (IWayless<TDestination, TSource>)mapper;
        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>()
            where TDestination : class
            where TSource : class
        {
            return GetNew<TDestination, TSource>(WaylessConfigurationBuilder.GetDefaultConfiguration<TDestination, TSource>());
        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>(IWaylessConfiguration configuration)
           where TDestination : class
           where TSource : class
        {
            var mapper = new Wayless<TDestination, TSource>(configuration);
            return mapper;
        }

    }
}