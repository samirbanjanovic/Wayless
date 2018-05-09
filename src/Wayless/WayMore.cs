using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Wayless.Core;

namespace Wayless
{
    public sealed class WayMore 
        : IWayMore
    {
        private static readonly Lazy<WayMore> _instance = new Lazy<WayMore>(() => new WayMore());

        private static readonly ConcurrentDictionary<object, object> _mappers = new ConcurrentDictionary<object, object>();

        public static IWayMore Wayless
        {
            get
            {
                return _instance.Value;
            }
        }

        public IWayMore ConfigureWayless<TDestination, TSource>(Action<IWayless<TDestination, TSource>> config)
            where TDestination : class
            where TSource : class
        {
            var mapperInstance = Get<TDestination, TSource>();
            config(mapperInstance);

            return this;
        }

        public IWayMore ConfigureWayless<TDestination, TSource>(IWaylessConfiguration configuration, Action<IWayless<TDestination, TSource>> config)
            where TDestination : class
            where TSource : class
        {
            var mapperInstance = Get<TDestination, TSource>(configuration);
            config(mapperInstance);

            return this;
        }

        public TDestination Map<TDestination, TSource>(TSource sourceObject)
            where TDestination : class
            where TSource : class
        {
            var mapper = Get<TDestination, TSource>();

            return mapper.Map(sourceObject);
        }

        public void Map<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class
        {
            var mapper = Get<TDestination, TSource>();

            mapper.Map(destinationObject, sourceObject);
        }

        public IEnumerable<TDestination> Map<TDestination, TSource>(IEnumerable<TSource> sourceObject)
            where TDestination : class
            where TSource : class
        {
            var mapper = Get<TDestination, TSource>();

            return mapper.Map(sourceObject);
        }


        public TDestination Map<TDestination, TSource>(TSource sourceObject, IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class
        {
            var mapper = Get<TDestination, TSource>(configuration);

            return mapper.Map(sourceObject);
        }

        public IEnumerable<TDestination> Map<TDestination, TSource>(IEnumerable<TSource> sourceObject, IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class
        {
            var mapper = Get<TDestination, TSource>(configuration);

            return mapper.Map(sourceObject);
        }

        public void Map<TDestination, TSource>(TDestination destinationObject, TSource sourceObject, IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class
        {
            var mapper = Get<TDestination, TSource>(configuration);

            mapper.Map(destinationObject, sourceObject);
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
            var key = (typeof(TDestination), typeof(TSource), configuration.ExpressionBuilder?.GetType(), configuration.MatchMaker?.GetType()).GetHashCode();
            if (!_mappers.TryGetValue(key, out object mapper))
            {
                mapper = GetNew<TDestination, TSource>();
                _mappers.TryAdd(key, mapper);
            }

            return (IWayless<TDestination, TSource>)mapper;
        }

        public IWayless<TDestination, TSource> Get<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class
        {
            return Get<TDestination, TSource>();
        }

        public IWayless<TDestination, TSource> Get<TDestination, TSource>(TDestination destinationObject, TSource sourceObject, IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class
        {
            return Get<TDestination, TSource>(configuration);
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
            return new Wayless<TDestination, TSource>(configuration);            
        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class
        {
            return GetNew<TDestination, TSource>();
        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>(TDestination destinationObject, TSource sourceObject, IWaylessConfiguration configuration)
            where TDestination : class
            where TSource : class
        {
            return GetNew<TDestination, TSource>(configuration);
        }


    }
}