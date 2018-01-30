using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Wayless.ExpressionBuilders
{
    public interface IExpressionBuilder
    {
        Action<TDestination, TSource> BuildActionMap<TDestination, TSource>(IEnumerable<Expression> mappingExpressions)
            where TDestination : class
            where TSource : class;
        Expression GetMapExpression<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression, Expression<Func<TSource, bool>> mapOnCondition = null)
            where TDestination : class
            where TSource : class;
        Expression GetMapExpression<TSource>(MemberInfo destinationMember, Expression<Func<TSource, object>> sourceExpression, Expression<Func<TSource, bool>> condition = null) where TSource : class;
        Expression GetMapExpression<TSource>(MemberInfo destinationMember, MemberInfo sourceMember, Expression<Func<TSource, bool>> condition = null);
        Expression GetMapExressionForExplicitSet<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, object value, Expression<Func<TSource, bool>> condition = null) where TDestination : class;
        Expression GetMapExressionForExplicitSet<TDestination>(Expression<Func<TDestination, object>> destinationExpression, object value) where TDestination : class;
        Expression GetMapExressionForExplicitSet<TSource>(MemberInfo destinationProperty, object value, Expression<Func<TSource, bool>> condition = null);
    }
}