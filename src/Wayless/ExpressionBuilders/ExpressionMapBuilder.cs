using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Wayless;

namespace Wayless.ExpressionBuilders
{
    internal class ExpressionMapBuilder
    {
        private readonly ParameterExpression _destination;
        private readonly ParameterExpression _source;

        public ExpressionMapBuilder(ParameterExpression destination, ParameterExpression source)
        {
            _destination = destination;
            _source = source;
        }

        // get expression for mapping property to property or property to function output
        public Expression GetMapExpression<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression
                                                                , Expression<Func<TSource, object>> sourceExpression
                                                                , Expression<Func<TSource, bool>> mapOnCondition = null)
            where TDestination : class
            where TSource : class
        {
            return GetMapExpression(destinationExpression.GetMemberInfo(), sourceExpression, mapOnCondition);
        }

        public Expression GetMapExpression<TSource>(MemberInfo destinationMember
                                                  , Expression<Func<TSource, object>> sourceExpression
                                                  , Expression<Func<TSource, bool>> condition = null)
            where TSource : class
        {
            MemberInfo sourceProperty = sourceExpression.GetMemberInfo();

            Expression expression = null;
            // assume function is not in form x => x.PropertyName
            if (sourceProperty == null)
            {
                expression = sourceExpression.AsExpressionWithInvokeSet(destinationMember, _destination, _source);
            }
            else
            {
                expression = BuildMapExpressionForValueMap(destinationMember, sourceProperty);
            }

            if (condition != null)
            {
                return expression.AsIfThenExpression(condition, _source);
            }

            return expression;
        }

        public Expression GetMapExpression<TSource>(MemberInfo destinationMember, MemberInfo sourceMember, Expression<Func<TSource, bool>> condition = null)
        {
            var expression = BuildMapExpressionForValueMap(destinationMember, sourceMember);

            if (condition != null)
            {
                return expression.AsIfThenExpression(condition, _source);
            }

            return expression;
        }

        #region helpers

        private Expression BuildMapExpressionForValueMap(MemberInfo destinationProperty, MemberInfo sourceProperty)
        {
            var expression = Expression.Assign(Expression.PropertyOrField(_destination, destinationProperty.Name)
                                                 , ExpressionBuilderHelpers.BuildCastExpression(Expression.PropertyOrField(_source, sourceProperty.Name), destinationProperty));


            return expression;
        }

        #endregion helpers
    }
}
