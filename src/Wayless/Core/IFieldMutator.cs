using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Wayless.Core
{
    public interface IFieldMutator<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        IFieldMutator<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression);
        IFieldMutator<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression, Expression<Func<TSource, bool>> mapOnCondition);
        IFieldMutator<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object fieldValue);
        IFieldMutator<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object value, Expression<Func<TSource, bool>> setCondition);
        IFieldMutator<TDestination, TSource> FieldSkip(Expression<Func<TDestination, object>> ignoreAtDestinationExpression);
    }
}