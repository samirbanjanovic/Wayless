using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Wayless.ExpressionBuilders;

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
            var key = (typeof((TDestination, TSource)), typeof(ExpressionBuilder));

            if (!_mappers.TryGetValue(key, out object mapper))
            {
                mapper = GetNew<TDestination, TSource>();
                _mappers.TryAdd((typeof((TDestination, TSource)), typeof(ExpressionBuilder)), mapper);
            }

            return (IWayless<TDestination, TSource>)mapper;
        }

        public IWayless<TDestination, TSource> Get<TDestination, TSource>(IExpressionBuilder expressionBuilder)
            where TDestination : class
            where TSource : class
        {
            var key = (typeof((TDestination, TSource)), expressionBuilder.GetType());

            if (!_mappers.TryGetValue(key, out object mapper))
            {
                mapper = GetNew<TDestination, TSource>(expressionBuilder);
                _mappers.TryAdd((typeof((TDestination, TSource)), expressionBuilder.GetType()), mapper);
            }

            return (IWayless<TDestination, TSource>)mapper;
        }
        
        public IWayless<TDestination, TSource> Get<TDestination, TSource>(TDestination destination, TSource source)
            where TDestination : class
            where TSource : class
        {
            return Get<TDestination, TSource>();
        }

        public IWayless<TDestination, TSource> Get<TDestination, TSource>(TDestination destination, TSource source, IExpressionBuilder expressionBuilder)
            where TDestination : class
            where TSource : class
        {
            return Get<TDestination, TSource>(expressionBuilder);
        }


        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>()
            where TDestination : class
            where TSource : class
        {
            return new Wayless<TDestination, TSource>();
        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>(IExpressionBuilder expressionBuilder)
           where TDestination : class
           where TSource : class
        {
            return new Wayless<TDestination, TSource>(expressionBuilder);
        }

        public IWayless<TDestination, TSource> GetNew<TDestination, TSource>(TDestination destination, TSource source)
            where TDestination : class
            where TSource : class
        {
            return GetNew<TDestination, TSource>();
        }             
    }
}