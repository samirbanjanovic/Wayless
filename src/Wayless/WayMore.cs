using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Wayless.Core;

namespace Wayless
{
    public sealed class WayMore 
        : IWayMore
    {
        private readonly ConcurrentDictionary<object, object> _mappers = new ConcurrentDictionary<object, object>();

        public WayMore() { }

        public IWayMore SetRules<TDestination, TSource>(Action<ISetRuleBuilder<TDestination, TSource>> mapperRules)
                    where TDestination : class
                    where TSource : class
        {            
            var setRuleBuilder = new SetRuleBuilder<TDestination, TSource>().UseDefaults();
            mapperRules(setRuleBuilder);

            var mapper = new Wayless<TDestination, TSource>(setRuleBuilder);

            AddOrUpdateMapper(mapper);

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


        public IWayless<TDestination, TSource> Get<TDestination, TSource>()
            where TDestination : class
            where TSource : class
        {
            var key = GenerateKey<TDestination, TSource>();
            if (!_mappers.TryGetValue(key, out object mapper))
            {
                var setRuleBuilder = new SetRuleBuilder<TDestination, TSource>().UseDefaults();

                mapper = new Wayless<TDestination, TSource>(setRuleBuilder);
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

        private void AddOrUpdateMapper<TDestination, TSource>(IWayless<TDestination, TSource> mapper)
            where TDestination : class
            where TSource : class
        {
            var key = GenerateKey<TDestination, TSource>();
            if (_mappers.ContainsKey(key))
            {
                _mappers[key] = mapper;
            }
            else
            {
                _mappers.TryAdd(key, mapper);
            }
        }

        private static int GenerateKey<TDestination, TSource>()
        {
            return (typeof(TDestination), typeof(TSource)).GetHashCode();
        }
    }
}