using System;
using System.Collections.Generic;

namespace Wayless
{
    public interface IWayless
    {
        Type DestinationType { get; }
        Type SourceType { get; }

        IWayless DontAutoMatchMembers();
        IWayless FieldMap<TDestination, TSource>(System.Linq.Expressions.Expression<Func<TDestination, object>> destinationExpression, System.Linq.Expressions.Expression<Func<TSource, object>> sourceExpression)
            where TDestination : class
            where TSource : class;
        IWayless FieldMap<TDestination, TSource>(System.Linq.Expressions.Expression<Func<TDestination, object>> destinationExpression, System.Linq.Expressions.Expression<Func<TSource, object>> sourceExpression, System.Linq.Expressions.Expression<Func<TSource, bool>> mapCondition)
            where TDestination : class
            where TSource : class;
        IWayless FieldSet<TDestination, TSource>(System.Linq.Expressions.Expression<Func<TDestination, object>> destinationExpression, object value)
            where TDestination : class
            where TSource : class;
        IWayless FieldSet<TDestination, TSource>(System.Linq.Expressions.Expression<Func<TDestination, object>> destinationExpression, object value, System.Linq.Expressions.Expression<Func<TSource, bool>> setCondition)
            where TDestination : class
            where TSource : class;
        IWayless FieldSkip<TDestination, TSource>(System.Linq.Expressions.Expression<Func<TDestination, object>> skipperName)
            where TDestination : class
            where TSource : class;
        IEnumerable<TDestination> Map<TDestination, TSource>(IEnumerable<TSource> sourceList)
            where TDestination : class
            where TSource : class;
        void Map<TDestination, TSource>(TDestination destinationObject, TSource sourceObject)
            where TDestination : class
            where TSource : class;
        TDestination Map<TDestination, TSource>(TSource sourceObject)
            where TDestination : class
            where TSource : class;
    }
}