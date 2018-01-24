using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless
{
    internal static class Extensions
    { 
        public static IDictionary<string, PropertyDetails<T>> GetPropertyDictionary<T>(this Type type)
        {
            return type.GetProperties()
                       .Select(pd => new PropertyDetails<T>(pd))
                       .ToDictionary(p => p.Name);
        }

        public static TMemberOut GetMember<T, TMemberOut>(this Expression<Func<T, object>> expression)
           where T : class
           where TMemberOut : class
        {
            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression = null;
            if (lambda.Body is UnaryExpression)
            {
                memberExpression = (lambda.Body as UnaryExpression)?.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            return memberExpression.Member as TMemberOut;
        }

        public static Action<Td, Ts> ExpressionMap<Td, Ts>(Expression<Func<Td, object>> destinationProperty, Expression<Func<Ts, object>> sourceProperty)
        {
            var destination = Expression.Parameter(typeof(Td), "destination");
            var source = Expression.Parameter(typeof(Ts), "source");     
                   
        }
    }
}
