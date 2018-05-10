using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Wayless.Core
{
    public interface IWayless<TDestination, TSource>        
        where TDestination : class
        where TSource : class
    {
        IWayless<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression);
        IWayless<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression, Expression<Func<TSource, bool>> mapOnCondition);
        IWayless<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object fieldValue);
        IWayless<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object value, Expression<Func<TSource, bool>> setCondition);
        IWayless<TDestination, TSource> FieldSkip(Expression<Func<TDestination, object>> ignoreAtDestinationExpression);

        Type DestinationType { get; }
        Type SourceType { get; }
        IEnumerable<TDestination> Map(IEnumerable<TSource> sourceList);
        void Map(TDestination destinationObject, TSource sourceObject);
        TDestination Map(TSource sourceObject);        
    }
}