using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless.ExpressionBuilders
{
    public sealed class ExpressionBuilder
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
        public Expression GetPropertyFieldMapExpression<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression)
            where TDestination : class
            where TSource : class
        {
            PropertyInfo destinationProperty = destinationExpression.GetMemberAsPropertyInfo();
            PropertyInfo sourceProperty = sourceExpression.GetMemberAsPropertyInfo();

            // assume function is not in form x => x.PropertyName
            if (sourceProperty == null)
            {
                return GetComplexMapExpression(destinationProperty, sourceExpression);
            }

            return GetPropertyFieldMapExpression(destinationProperty, sourceProperty);
        }

        // get expression for property to function output
        public Expression GetComplexMapExpression<TSource>(MemberInfo destinationProperty, Expression<Func<TSource, object>> sourceExpression)
            where TSource : class
        {
            var expression = Expression.Assign(Expression.PropertyOrField(_destination, destinationProperty.Name)
                                             , BuildCastExpression(Expression.Invoke(sourceExpression, _source), destinationProperty));

            return expression;
        }

        public Expression GetPropertyFieldMapExpression(MemberInfo destinationProperty, MemberInfo sourceProperty)
        {
            var expression = Expression.Assign(Expression.PropertyOrField(_destination, destinationProperty.Name) 
                                                 , BuildCastExpression(Expression.PropertyOrField(_source, sourceProperty.Name), destinationProperty)); 

            return expression;
        }

        public Expression GetPropertyFieldSetExression<TDestination>(Expression<Func<TDestination, object>> destinationExpression, object value)
            where TDestination : class
        {
            return GetPropertyFieldSetExression(destinationExpression, value);
        }

        public Expression GetPropertyFieldSetExression(MemberInfo destinationProperty, object value)
        {
            var expression = Expression.Assign(Expression.PropertyOrField(_destination, destinationProperty.Name)
                                              , BuildCastExpression(Expression.Constant(value), destinationProperty));

            return expression;
        }

        #region helpers
        private Expression BuildCastExpression(Expression valueExpression, MemberInfo destinationProperty)
        {
            var destinationType = destinationProperty.GetUnderlyingType();

            if (destinationType.IsValueType)
            {
                return Expression.Convert(valueExpression, destinationType);
            }
            
            return Expression.TypeAs(valueExpression, destinationType);
        }
        #endregion helpers
    }
}
