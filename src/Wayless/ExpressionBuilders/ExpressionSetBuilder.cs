using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Wayless.ExpressionBuilders
{
    internal class ExpressionSetBuilder
    {
        private readonly ParameterExpression _destination;

        public ParameterExpression _source { get; }

        public ExpressionSetBuilder(ParameterExpression destination, ParameterExpression source)
        {
            _destination = destination;
            _source = source;
        }


        public Expression GetMapExressionForExplicitSet<TDestination>(Expression<Func<TDestination, object>> destinationExpression, object value)
            where TDestination : class
        {
            return GetMapExressionForExplicitSet<object>(destinationExpression.GetMemberInfo(), value, null);
        }

        public Expression GetMapExressionForExplicitSet<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, object value, Expression<Func<TSource, bool>> condition = null)
            where TDestination : class
        {
            var expression = GetMapExressionForExplicitSet(destinationExpression.GetMemberInfo(), value, condition);

            return expression;
        }

        public Expression GetMapExressionForExplicitSet<TSource>(MemberInfo destinationProperty, object value, Expression<Func<TSource, bool>> condition = null)
        {
            var expression = Expression.Assign(Expression.PropertyOrField(_destination, destinationProperty.Name)
                                              , ExpressionBuilderHelpers.BuildCastExpression(Expression.Constant(value), destinationProperty));


            if (condition != null)
            {
                return expression.AsIfThenExpression(condition, _source);
            }

            return expression;
        }
    }
}