using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Wayless
{
    public static class WayMore<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        private static readonly IDictionary<object, Action<TDestination, TSource>> _waylessMappers = new Dictionary<object, Action<TDestination, TSource>>();

        public static void DoLess(TDestination destination, TSource source)
        {
            try
            {
                _waylessMappers[destination](destination, source);
            }
            catch (KeyNotFoundException x)
            {
                var mapper = new WaylessMap<TDestination, TSource>();
                _waylessMappers.Add(destination, MappingExpression.BuildMapperAccess(mapper));
                InitialDoLess(destination, source);
            }
        }

        private static void InitialDoLess(TDestination destination, TSource source)
        {
            _waylessMappers[destination](destination, source);
        }
    }
}
