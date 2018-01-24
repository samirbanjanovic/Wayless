using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


namespace Wayless
{
    public static class MappingExpression
    {
        //Expression.TypeAs(, setExpression.PropertyType)
        public static Action<TDestination, TSource> Build<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression)
            where TDestination : class
            where TSource : class
        {
            var destination = Expression.Parameter(typeof(TDestination), "destination");
            var source = Expression.Parameter(typeof(TSource), "source");


            PropertyInfo destinationProperty = destinationExpression.GetMember<TDestination, PropertyInfo>();
            PropertyInfo sourceProperty = sourceExpression.GetMember<TSource, PropertyInfo>();

            var assign = Expression.Lambda<Action<TDestination, TSource>>(Expression.Call(destination, destinationProperty.GetSetMethod(),
                                    Expression.Call(source, sourceProperty.GetGetMethod())), new ParameterExpression[] { destination, source }).Compile();

            return assign;
        }

        public static Action<TDestination, TSource> Build<TDestination, TSource>(PropertyInfo destinationProperty, PropertyInfo sourceProperty)
            where TDestination : class
            where TSource : class
        {
            var destination = Expression.Parameter(typeof(TDestination), "destination");
            var source = Expression.Parameter(typeof(TSource), "source");

            var assign = Expression.Lambda<Action<TDestination, TSource>>(Expression.Call(destination, destinationProperty.GetSetMethod(),
                                    Expression.Call(source, sourceProperty.GetGetMethod())), new ParameterExpression[] { destination, source }).Compile();

            return assign;
        }

        public static Action<TDestination, TSource> Build<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, object value)
            where TDestination : class
            where TSource : class
        {
            var destination = Expression.Parameter(typeof(TDestination), "destination");
            var source = Expression.Parameter(typeof(TSource), "source");

            PropertyInfo destinationProperty = destinationExpression.GetMember<TDestination, PropertyInfo>();

            // pass source object but ignore it so it can be used in same mapping dictionary
            var assign = Expression.Lambda<Action<TDestination, TSource>>(Expression.Call(destination, destinationProperty.GetSetMethod(), Expression.Constant(value)), new ParameterExpression[] { destination, source }).Compile();

            return assign;
        }
    }
}
