using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Wayless
{
    public interface IWayless<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        Type DestinationType { get; }
        Type SourceType { get; }

        IWayless<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression);
        IWayless<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object fieldValue);
        IWayless<TDestination, TSource> FieldSkip(Expression<Func<TDestination, object>> ignoreAtDestinationExpression);
        IWayless<TDestination, TSource> FieldRestore(Expression<Func<TDestination, object>> restoreAtDestinationExpression);
        IEnumerable<TDestination> Map(IEnumerable<TSource> sourceList);
        void Map(TDestination destinationObject, TSource sourceObject);
        TDestination Map(TSource sourceObject);        
    }
}