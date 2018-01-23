using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless
{
    internal class PropertyDetails<T>         
    {
        public PropertyDetails(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            Name = PropertyInfo.Name;            
        }

        public string Name { get; }

        public PropertyInfo PropertyInfo { get; }

        private Action<T, object> _valueSet;
        public Action<T, object> ValueSet
        {
            get
            {
                if(_valueSet == null)
                {
                    _valueSet = InitializeSet(PropertyInfo);
                }

                return _valueSet;
            }
        }

        private Func<T, object> _valueGet;
        public Func<T, object> ValueGet
        {
            get
            {
                if (_valueGet == null)
                {
                    _valueGet = InitializeGet(PropertyInfo);
                }

                return _valueGet;
            }
        }

        private static Action<T, object> InitializeSet(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(T), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            UnaryExpression valueCast = propertyInfo.PropertyType.IsValueType
                ? Expression.Convert(value, propertyInfo.PropertyType)
                : Expression.TypeAs(value, propertyInfo.PropertyType);


            return Expression.Lambda<Action<T, object>>(Expression.Call(instance, propertyInfo.GetSetMethod(), valueCast), new ParameterExpression[] { instance, value }).Compile();
        }

        private static Func<T, object> InitializeGet(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(T), "instance");

            return Expression.Lambda<Func<T, object>>(Expression.TypeAs(Expression.Call(instance, propertyInfo.GetGetMethod()), typeof(object)), instance).Compile();

        }
    }
}
