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

        public IWaylessConfiguration WithEmptyConfiguration()
        {
            return WaylessConfigurationBuilder.EmptyConfiguration();
        }
        
        public IWaylessConfiguration WithDefaultConfiguration<TDestination, TSource>()
        {
            return WaylessConfigurationBuilder.DefaultConfiguration<TDestination, TSource>();
        }

        public IWaylessConfiguration WithDefaultConfiguration(Type destinationType, Type sourceType)
        {
            return WaylessConfigurationBuilder.DefaultConfiguration(destinationType, sourceType);
        }

        public 
    }
}