using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


namespace Wayless
{
    public static class MappingExpression
    {
        public static Action<TDestination, TSource> Build<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression)
            where TDestination : class
            where TSource : class
        {
            var destination = Expression.Parameter(typeof(TDestination), "destination");
            var source = Expression.Parameter(typeof(TSource), "source");


            PropertyInfo destinationProperty = destinationExpression.GetMember<TDestination, PropertyInfo>();
            PropertyInfo sourceProperty = sourceExpression.GetMember<TSource, PropertyInfo>();

            var assign = Expression.Lambda<Action<TDestination, TSource>>(Expression.Call(destination, destinationProperty.GetSetMethod(),
                                    Expression.Call(source, sourceProperty.GetGetMethod()))
                                    , new ParameterExpression[] 
                                    {
                                        destination
                                        , source
                                    })
                                    .Compile();

            return assign;
        }

        public static Action<TDestination, TSource> Build<TDestination, TSource>(PropertyInfo destinationProperty, PropertyInfo sourceProperty)
            where TDestination : class
            where TSource : class
        {
            return BuildExpression<TDestination, TSource>(destinationProperty, sourceProperty).Compile();
        }

        public static Action<TDestination, TSource> Build<TDestination, TSource>(Expression<Func<TDestination, object>> destinationExpression, object value)
            where TDestination : class
            where TSource : class
        {
            var destination = Expression.Parameter(typeof(TDestination), "destination");
            var source = Expression.Parameter(typeof(TSource), "source");

            PropertyInfo destinationProperty = destinationExpression.GetMember<TDestination, PropertyInfo>();

            // pass source object but ignore it so it can be used in same mapping dictionary
            var assign = Expression.Lambda<Action<TDestination, TSource>>(
                                    Expression.Call(destination, destinationProperty.GetSetMethod()
                                        , Expression.Convert(Expression.Constant(value), destinationProperty.PropertyType), destination, source)
                                    , new ParameterExpression[] 
                                    {
                                        destination
                                        , source
                                    })
                                    .Compile();

            return assign;
        }

        public static Action<TDestination, TSource> BuildMapperAccess<TDestination, TSource>(IWaylessMap<TDestination, TSource> mapper)
            where TDestination : class
            where TSource : class
        {
            var destination = Expression.Parameter(typeof(TDestination), "destination");
            var source = Expression.Parameter(typeof(TSource), "source");


            var mapCall = Expression.Lambda<Action<TDestination, TSource>>(
                                    Expression.Call(Expression.Constant(mapper), mapper.GetType().GetMethod("Map", new Type[] { typeof(TDestination), typeof(TSource) }),  destination, source)
                                    , new ParameterExpression[]
                                    {
                                        destination
                                        , source
                                    })
                                    .Compile();

            return mapCall;
        }

        public static Action<TDestination, TSource> BuildMap<TDestination, TSource>(IEnumerable<(PropertyInfo, PropertyInfo)> properties)
            where TDestination : class
            where TSource : class
        {
            var destination = Expression.Parameter(typeof(TDestination), "destination");
            var source = Expression.Parameter(typeof(TSource), "source");

            var expressions = new List<Expression>();
            foreach (var pair in properties)
            {
                expressions.Add(Expression.Call(destination, pair.Item1.GetSetMethod()
                                        , Expression.Convert(Expression.Call(source, pair.Item2.GetGetMethod()), pair.Item2.PropertyType)));
            }

            var assign = Expression.Lambda<Action<TDestination, TSource>>(
                                   Expression.Block(
                                    expressions)
                                   , new ParameterExpression[]
                                    {
                                        destination
                                        , source
                                    })
                                    .Compile();

            return assign;
        }        

        public static Expression<Action<TDestination, TSource>> BuildExpression<TDestination, TSource>(PropertyInfo destinationProperty, PropertyInfo sourceProperty)
            where TDestination : class
            where TSource : class
        {
            var destination = Expression.Parameter(typeof(TDestination), "destination");
            var source = Expression.Parameter(typeof(TSource), "source");

            var assign = Expression.Lambda<Action<TDestination, TSource>>(
                                    Expression.Call(destination, destinationProperty.GetSetMethod()
                                        , Expression.Convert(Expression.Call(source, sourceProperty.GetGetMethod()), destinationProperty.PropertyType))
                                    , new ParameterExpression[]
                                    {
                                        destination
                                        , source
                                    });
            
            return assign;
        }
    }

    
}
