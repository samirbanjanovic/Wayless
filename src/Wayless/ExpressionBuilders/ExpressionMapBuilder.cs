using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless.ExpressionBuilders
{
    public sealed class ExpressionMapBuilder<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        private readonly ParameterExpression _destination = Expression.Parameter(typeof(TDestination), "destination");
        private readonly ParameterExpression _source = Expression.Parameter(typeof(TSource), "source");

        /// <summary>
        /// Build a unified mapping function
        /// </summary>
        /// <param name="mappingExpressions">Expressions containting member mappings</param>
        /// <returns>mapping action</returns>
        public Action<TDestination, TSource> BuildActionMap(IEnumerable<Expression> mappingExpressions)
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
        public Expression GetPropertyFieldMapExpression(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression)
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
        public Expression GetComplexMapExpression(PropertyInfo destinationProperty, Expression<Func<TSource, object>> sourceExpression)
        {
            return Expression.Call(_destination, destinationProperty.GetSetMethod()
                             , Expression.Convert(Expression.Invoke(sourceExpression, _source), destinationProperty.PropertyType));
        }

        public Expression GetPropertyFieldMapExpression(PropertyInfo destinationProperty, PropertyInfo sourceProperty)
        {
            var callExpression = Expression.Call(_destination, destinationProperty.GetSetMethod()
                                           , Expression.Convert(Expression.Call(_source, sourceProperty.GetGetMethod()), destinationProperty.PropertyType));

            return callExpression;
        }

        public Expression GetPropertyFieldSetExression(Expression<Func<TDestination, object>> destinationExpression, object value)
        {
            return GetPropertyFieldSetExression(destinationExpression, value);
        }

        public Expression GetPropertyFieldSetExression(PropertyInfo destinationProperty, object value)
        {
            return Expression.Call(_destination, destinationProperty.GetSetMethod()
                             , Expression.Convert(Expression.Constant(value), destinationProperty.PropertyType));
        }
    }
}
