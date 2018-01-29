using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless.ExpressionBuilders
{
    internal sealed class ExpressionBuilder
    {
        private readonly ParameterExpression _destination;
        private readonly ParameterExpression _source;
        private readonly ExpressionSetBuilder _expressionSetBuilder;
        public ExpressionBuilder(Type destinationType, Type sourceType)
        {
             _destination = Expression.Parameter(destinationType, "destination");
            _source = Expression.Parameter(sourceType, "source");

            _expressionSetBuilder = new ExpressionSetBuilder(_destination, _source);
        }

        /// <summary>
        /// Build a unified mapping function
        /// </summary>
        /// <param name="mappingExpressions">Expressions containting member mappings</param>
        /// <returns>mapping action</returns>
        public Action<TDestination, TSource> BuildActionMap<TDestination, TSource>(IEnumerable<Expression> mappingExpressions)
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
                //sourceExpression.AsIfThenExpression(destinationMember, condition, _source);
                // BuildExpressionForSourceExpression(destinationMember, sourceExpression);
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

        #region explicit set
        public Expression GetMapExressionForExplicitSet<TDestination>(Expression<Func<TDestination, object>> destinationExpression, object value)
            where TDestination : class
        {
            var expression =  _expressionSetBuilder.GetMapExressionForExplicitSet(destinationExpression, value);

            return expression;
        }

        public Expression GetMapExressionForExplicitSet<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, object value, Expression<Func<TSource, bool>> condition = null)
            where TDestination : class
        {
            var expression = _expressionSetBuilder.GetMapExressionForExplicitSet(destinationExpression, value, condition);

            return expression;
        }

        public Expression GetMapExressionForExplicitSet<TSource>(MemberInfo destinationProperty, object value, Expression<Func<TSource, bool>> condition = null)
        {
            var expression = _expressionSetBuilder.GetMapExressionForExplicitSet(destinationProperty, value, condition);
           
            return expression;
        }

        #endregion explicit set

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
