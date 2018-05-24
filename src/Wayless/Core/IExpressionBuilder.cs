using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Wayless.Core
{
    public interface IExpressionBuilder
    {
        Action<TDestination, TSource> CompileExpressionMap<TDestination, TSource>(IEnumerable<Expression> mappingExpressions)
            where TDestination : class
            where TSource : class;

        Expression GetMapExpression<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression, Expression<Func<TSource, bool>> condition = null)
            where TDestination : class
            where TSource : class;

        Expression GetMapExpression<TSource>(MemberInfo destinationMember, Expression<Func<TSource, object>> sourceExpression, Expression<Func<TSource, bool>> condition = null)
            where TSource : class;

        Expression GetMapExpression<TSource>(MemberInfo destinationMember, MemberInfo sourceMember, Expression<Func<TSource, bool>> condition = null)
            where TSource : class;

        Expression GetMapExressionForExplicitSet<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, object value, Expression<Func<TSource, bool>> condition = null)
            where TDestination : class
            where TSource : class;

        Expression GetMapExressionForExplicitSet<TDestination>(Expression<Func<TDestination, object>> destinationExpression, object value)
            where TDestination : class;

        Expression GetMapExressionForExplicitSet<TSource>(MemberInfo destinationProperty, object value, Expression<Func<TSource, bool>> condition = null)
            where TSource : class;
    }
}