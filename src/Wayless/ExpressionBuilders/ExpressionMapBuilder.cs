using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless.ExpressionBuilders
{
    public class ExpressionMapBuilder<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        ParameterExpression _destination;
        ParameterExpression _source;

        public ExpressionMapBuilder()
        {
            _destination = Expression.Parameter(typeof(TDestination), "destination");
            _source = Expression.Parameter(typeof(TSource), "source");
        }

        public Action<TDestination, TSource> BuildActionMap(IEnumerable<Expression> mappingExpressions)
        {
            var mapping = Expression.Lambda<Action<TDestination, TSource>>(
                                   Expression.Block(mappingExpressions)
                                   , new ParameterExpression[]
                                    {
                                        _destination
                                        , _source
                                    })
                                    .Compile();

            return mapping;
        }

        // get expression for mapping property to property or property to function output
        public Expression GetPropertyFieldMapExpression(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression)
        {
            PropertyInfo destinationProperty = destinationExpression.GetMember<TDestination, PropertyInfo>();
            PropertyInfo sourceProperty = sourceExpression.GetMember<TSource, PropertyInfo>();

            // assume function is not in form x => x.PropertyName
            if (sourceProperty == null)
            {
                return GetComplexMapExpression(destinationProperty, sourceExpression);                
            }

            //PropertyInfo sourceProperty = sourceExpression.GetMember<TSource, PropertyInfo>();

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
