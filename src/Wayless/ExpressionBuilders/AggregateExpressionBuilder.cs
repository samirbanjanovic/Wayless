using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless.ExpressionBuilders
{
    internal sealed class AggregateExpressionBuilder 
        : IExpressionBuilder
    {
        private readonly ParameterExpression _destination;
        private readonly ParameterExpression _source;

        private readonly ExpressionSetBuilder _expressionSetBuilder;
        private readonly ExpressionMapBuilder _expressionMapBuilder;
        public AggregateExpressionBuilder(Type destinationType, Type sourceType)
        {
             _destination = Expression.Parameter(destinationType, "destination");
            _source = Expression.Parameter(sourceType, "source");

            _expressionSetBuilder = new ExpressionSetBuilder(_destination, _source);
            _expressionMapBuilder = new ExpressionMapBuilder(_destination, _source);
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
            return _expressionMapBuilder.GetMapExpression(destinationExpression.GetMemberInfo(), sourceExpression, mapOnCondition);            
        }

        public Expression GetMapExpression<TSource>(MemberInfo destinationMember
                                                  , Expression<Func<TSource, object>> sourceExpression
                                                  , Expression<Func<TSource, bool>> condition = null)
            where TSource : class
        {
            var expression = _expressionMapBuilder.GetMapExpression(destinationMember, sourceExpression, condition);

            return expression;
        }

        public Expression GetMapExpression<TSource>(MemberInfo destinationMember, MemberInfo sourceMember, Expression<Func<TSource, bool>> condition = null)
        {
            var expression = _expressionMapBuilder.GetMapExpression(destinationMember, sourceMember, condition);

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
