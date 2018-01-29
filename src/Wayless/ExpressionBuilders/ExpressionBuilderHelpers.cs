using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Wayless.ExpressionBuilders
{
    internal static class ExpressionBuilderHelpers
    {
        public static Expression AsIfThenExpression<TSource>(this Expression statement
                                                           , Expression<Func<TSource, bool>> condition
                                                           , ParameterExpression source)
        {
            var member = (condition.Body as MemberExpression)?.Member as MemberInfo;
            Expression booleanExpression;
            if (member == null)
            {
                booleanExpression = Expression.Invoke(condition, source);
            }
            else
            {
                booleanExpression = Expression.PropertyOrField(source, member.Name);
            }

            return Expression.IfThen(booleanExpression, statement);
        }

        public static Expression AsIfThenExpression<TSource>(this MemberInfo member
                                                           , Expression<Func<TSource, bool>> condition
                                                           , ParameterExpression source)
        {            
            return Expression.IfThen(condition, Expression.PropertyOrField(source, member.Name));
        }

        public static Expression AsExpressionWithInvokeSet<TSource>(this Expression<Func<TSource, object>> sourceExpression
                                                                  , MemberInfo destinationMember
                                                                  , ParameterExpression destination
                                                                  , ParameterExpression source)
            where TSource : class
        {
            var expression = Expression.Assign(Expression.PropertyOrField(destination, destinationMember.Name)
                                             , BuildCastExpression(Expression.Invoke(sourceExpression, source), destinationMember));

            return expression;
        }

        public static Expression AsExpressionForValueMap(MemberInfo destinationProperty
                                                             , MemberInfo sourceProperty
                                                             , ParameterExpression destination
                                                             , ParameterExpression source)
        {
            var expression = Expression.Assign(Expression.PropertyOrField(destination, destinationProperty.Name)
                                                 , BuildCastExpression(Expression.PropertyOrField(source, sourceProperty.Name), destinationProperty));


            return expression;
        }

        public static Expression BuildCastExpression(Expression valueExpression, MemberInfo destinationProperty)
        {
            var destinationType = destinationProperty.GetUnderlyingType();

            if (destinationType.IsValueType)
            {
                return Expression.Convert(valueExpression, destinationType);
            }
            
            return Expression.TypeAs(valueExpression, destinationType);
        }
    }
}