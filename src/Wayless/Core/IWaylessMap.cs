using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Wayless
{
    public interface IWaylessMap<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        Type DestinationType { get; }
        Type SourceType { get; }

        IWaylessMap<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression);
        IWaylessMap<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object fieldValue);
        IWaylessMap<TDestination, TSource> FieldSkip(Expression<Func<TDestination, object>> ignoreAtDestinationExpression);
        IEnumerable<TDestination> Map(IEnumerable<TSource> sourceList, params object[] constructorParameters);
        TDestination Map(params object[] constructorParameters);
        void Map(ref TDestination destinationObject, ref TSource sourceObject);
        void Map(TDestination destinationObject);
        void Map(TDestination destinationObject, TSource sourceObject);
        TDestination Map(TSource sourceObject, params object[] constructorParameters);
        IEnumerable<string> ShowMapping();
    }
}