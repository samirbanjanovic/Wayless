using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless
{
    public class PropertyInfoPair<TDestination, TSource> 
        : IPropertyInfoPair
    {
        public IPropertyDetails DestinationProperty { get; set; }

        public IPropertyDetails SourceProperty { get; set; }

        public Action<TDestination, object> Setter { get; set; }

        public Func<TSource, object> Getter { get; set; }


        internal void InitializeSet()
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            // value as T is slightly faster than (T)value, so if it's not a value type, use that
            UnaryExpression instanceCast = this.DestinationProperty.PropertyInfo.DeclaringType.IsValueType
                ? Expression.Convert(instance, this.DestinationProperty.PropertyInfo.DeclaringType)
                : Expression.TypeAs(instance, this.DestinationProperty.PropertyInfo.DeclaringType);

            UnaryExpression valueCast = this.DestinationProperty.PropertyInfo.PropertyType.IsValueType 
                ? Expression.Convert(value, this.DestinationProperty.PropertyInfo.PropertyType)
                : Expression.TypeAs(value, this.DestinationProperty.PropertyInfo.PropertyType);

            Setter = Expression.Lambda<Action<TDestination, object>>(Expression.Call(instanceCast, this.DestinationProperty.PropertyInfo.GetSetMethod(), valueCast), new ParameterExpression[] { instance, value }).Compile();

        }

        internal void InitializeGet()
        {
            var instance = Expression.Parameter(typeof(object), "instance");

            UnaryExpression instanceCast = this.SourceProperty.PropertyInfo.DeclaringType.IsValueType
                ? Expression.Convert(instance, this.SourceProperty.PropertyInfo.DeclaringType)
                : Expression.TypeAs(instance, this.SourceProperty.PropertyInfo.DeclaringType);

            Getter = Expression.Lambda<Func<TSource, object>>(Expression.TypeAs(Expression.Call(instanceCast, this.SourceProperty.PropertyInfo.GetGetMethod()), typeof(object)), instance).Compile();

        }
    }
}
