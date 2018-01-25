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
        IEnumerable<TDestination> Map(IEnumerable<TSource> sourceList);
        void Map(TDestination destinationObject, TSource sourceObject);
        TDestination Map(TSource sourceObject);        
    }
}