using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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

        private readonly Hashtable _mappers = new Hashtable();
        

        public IWaylessMap<TDestination, TSource> Get<TDestination, TSource>()
            where TDestination : class
            where TSource : class
        {
            var key = (typeof((TDestination, TSource)));
            if (_mappers.ContainsKey(key))
            {
                return (IWaylessMap<TDestination, TSource>)_mappers[key];
            }
            else
            {
                var mapper = new WaylessMap<TDestination, TSource>();
                _mappers.Add(typeof((TDestination, TSource)), mapper);

                return mapper;
            }
        }
    }
}
