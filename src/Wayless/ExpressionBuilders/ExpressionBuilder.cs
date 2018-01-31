using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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

        public virtual Expression GetMapExpression<TSource>(MemberInfo destinationMember, MemberInfo sourceMember, Expression<Func<TSource, bool>> condition = null)
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
        #endregion create assignment map

        #region create set map
        public virtual Expression GetMapExressionForExplicitSet<TDestination>(Expression<Func<TDestination, object>> destinationExpression, object value)
            where TDestination : class
        {
            return GetMapExressionForExplicitSet<object>(destinationExpression.GetMemberInfo(), value, null);
        }

        public virtual Expression GetMapExressionForExplicitSet<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, object value, Expression<Func<TSource, bool>> condition = null)
            where TDestination : class
        {
            var expression = GetMapExressionForExplicitSet(destinationExpression.GetMemberInfo(), value, condition);

            return expression;
        }

        public virtual Expression GetMapExressionForExplicitSet<TSource>(MemberInfo destinationProperty, object value, Expression<Func<TSource, bool>> condition = null)
        {
            var expression = Expression.Assign(Expression.PropertyOrField(_destination, destinationProperty.Name)
                                              , ExpressionBuilderHelpers.BuildCastExpression(Expression.Constant(value), destinationProperty));


            if (condition != null)
            {
                return expression.AsIfThenExpression(condition, _source);
            }

            return expression;
        }
        #endregion create set map
    }
}
