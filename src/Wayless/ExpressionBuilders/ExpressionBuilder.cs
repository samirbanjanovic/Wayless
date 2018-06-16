using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Wayless.Core;

namespace Wayless
{
    public class ExpressionBuilder
       : IExpressionBuilder
    {
        private readonly ParameterExpression _destination;
        private readonly ParameterExpression _source;


        public ExpressionBuilder(Type destinationType, Type sourceType)
        {
            _destination = Expression.Parameter(destinationType, "destination");
            _source = Expression.Parameter(sourceType, "source");
        }

        /// <summary>
        /// Build a unified mapping function
        /// </summary>
        /// <param name="mappingExpressions">Expressions containting member mappings</param>
        /// <returns>mapping action</returns>
        public virtual Action<TDestination, TSource> CompileExpressionMap<TDestination, TSource>(IEnumerable<Expression> mappingExpressions)
            where TDestination : class
            where TSource : class
        {
            var expressionMap = Expression.Lambda<Action<TDestination, TSource>>(
                                   Expression.Block(mappingExpressions)
                                   , new ParameterExpression[]
                                    {
                                        _destination
                                        , _source
                                    });


            return expressionMap.Compile();
        }


        #region create assignment map
        // get expression for mapping property to property or property to function output
        public virtual Expression GetMapExpression<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression
                                                                , Expression<Func<TSource, object>> sourceExpression
                                                                , Expression<Func<TSource, bool>> mapOnCondition = null)
            where TDestination : class
            where TSource : class
        {
            return GetMapExpression(destinationExpression.GetMemberInfo(), sourceExpression, mapOnCondition);
        }

        public virtual Expression GetMapExpression<TSource>(MemberInfo destinationMember
                                                  , Expression<Func<TSource, object>> sourceExpression
                                                  , Expression<Func<TSource, bool>> condition = null)
            where TSource : class
        {
            MemberInfo sourceProperty = sourceExpression.GetMemberInfo();

            Expression expression = null;
            // assume function is not in form x => x.PropertyName
            if (sourceProperty == null)
            {
                expression = Expression.Assign(Expression.PropertyOrField(_destination, destinationMember.Name)
                                             , BuildCastExpression(Expression.Invoke(sourceExpression, _source), destinationMember));

            }
            else
            {
                expression = BuildMapExpressionForValueMap(destinationMember, sourceProperty);
            }

            if (condition != null)
            {
                return AsIfThenExpression(expression, condition);
            }

            return expression;
        }

        public virtual Expression GetMapExpression<TSource>(MemberInfo destinationMember, MemberInfo sourceMember, Expression<Func<TSource, bool>> condition = null)
            where TSource : class
        {
            var expression = BuildMapExpressionForValueMap(destinationMember, sourceMember);

            if (condition != null)
            {
                return AsIfThenExpression(expression, condition);
            }

            return expression;
        }

        #endregion create assignment map

        #region create set map
        public virtual Expression GetMapExressionForExplicitSet<TDestination>(Expression<Func<TDestination, object>> destinationExpression, object value)
            where TDestination : class
        {
            return GetMapExressionForExplicitSet<object>(destinationExpression.GetMemberInfo(), value, null);
        }

        public virtual Expression GetMapExressionForExplicitSet<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, object value, Expression<Func<TSource, bool>> condition = null)
            where TDestination : class
            where TSource : class
        {
            var expression = GetMapExressionForExplicitSet(destinationExpression.GetMemberInfo(), value, condition);

            return expression;
        }

        public virtual Expression GetMapExressionForExplicitSet<TSource>(MemberInfo destinationProperty, object value, Expression<Func<TSource, bool>> condition = null)
            where TSource : class
        {
            Expression assignmentExpression = Expression.Constant(value);

            if(destinationProperty.GetUnderlyingType().IsAssignableFrom(value.GetType()))
            {
                assignmentExpression = BuildCastExpression(assignmentExpression, destinationProperty);
            }

            var expression = Expression.Assign(Expression.PropertyOrField(_destination, destinationProperty.Name), assignmentExpression);


            if (condition != null)
            {
                return AsIfThenExpression(expression, condition);
            }

            return expression;
        }

        public virtual Expression GetMapExressionForExplicitSet<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, Expression value, Expression<Func<TSource, bool>> condition = null)
            where TDestination : class
            where TSource : class
        {
            var expression = GetMapExressionForExplicitSet(destinationExpression.GetMemberInfo(), value, condition);

            return expression;
        }

        public virtual Expression GetMapExressionForExplicitSet<TSource>(MemberInfo destinationProperty, Expression value, Expression<Func<TSource, bool>> condition = null)
            where TSource : class
        {
            Expression valueExpression = Expression.Invoke(value);

            

            var expression = Expression.Assign(Expression.PropertyOrField(_destination, destinationProperty.Name), valueExpression);

            if (condition != null)
            {
                return AsIfThenExpression(expression, condition);
            }

            return expression;
        }
        #endregion create set map


        #region helpers

        private Expression BuildMapExpressionForValueMap(MemberInfo destinationProperty, MemberInfo sourceProperty)
        {
            Expression assignmentExpression = Expression.PropertyOrField(_source, sourceProperty.Name);

            if(!destinationProperty.GetUnderlyingType().IsAssignableFrom(sourceProperty.GetUnderlyingType()))
            {
                assignmentExpression = BuildCastExpression(assignmentExpression, destinationProperty);
            }

            var expression = Expression.Assign(Expression.PropertyOrField(_destination, destinationProperty.Name)
                                             , assignmentExpression);


            return expression;
        }

        private Expression BuildCastExpression(Expression valueExpression, MemberInfo destinationProperty)
        {
            Type destinationType = destinationProperty.GetUnderlyingType();

            if (destinationType.IsValueType)
            {
                return Expression.Convert(valueExpression, destinationType);
            }

            return Expression.TypeAs(valueExpression, destinationType);
        }

        public Expression AsIfThenExpression<TSource>(Expression statement
                                                           , Expression<Func<TSource, bool>> condition)
            where TSource : class
        {
            var member = (condition.Body as MemberExpression)?.Member as MemberInfo;
            Expression booleanExpression;
            if (member == null)
            {
                booleanExpression = Expression.Invoke(condition, _source);
            }
            else
            {
                booleanExpression = Expression.PropertyOrField(_source, member.Name);
            }

            return Expression.IfThen(booleanExpression, statement);
        }
        #endregion helpers
    }
}
