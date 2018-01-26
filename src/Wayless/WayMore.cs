using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Wayless.Generic;

namespace Wayless
{
    public sealed class WayMore
    {
        private static readonly Lazy<WayMore> _instance = new Lazy<WayMore>(() => new WayMore());

        public static WayMore Mappers
        {
            get
            {
                return _instance.Value;
            }
        }

        private readonly ConcurrentDictionary<object, object> _mappers = new ConcurrentDictionary<object, object>();

        public IWayless<TDestination, TSource> Get<TDestination, TSource>()
            where TDestination : class
            where TSource : class
        {
            var key = (typeof((TDestination, TSource)));
            if (!_mappers.TryGetValue(key, out object mapper))
            {
                mapper = GetNew<TDestination, TSource>();
                _mappers.TryAdd(typeof((TDestination, TSource)), mapper);
            }

            return (IWayless<TDestination, TSource>)mapper;

        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>()
            where TDestination : class
            where TSource : class
        {
            return new Wayless<TDestination, TSource>();
        }
    }
}
