using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Wayless
{
    public static class Extensions
    { 
        public static IDictionary<string, IPropertyDetails> GetPropertyDictionary(this Type type, bool useInvarientKey = false)
        {
            return type.GetProperties().Select(x => new PropertyDetails(x) as IPropertyDetails).ToDictionary(p => useInvarientKey ? p.InvarientName : p.Name);
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
    }
}
