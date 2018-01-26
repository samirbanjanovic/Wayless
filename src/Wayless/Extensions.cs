using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless
{
    public static class Extensions
    {
        public static IDictionary<string, PropertyInfo> GetInvariantPropertyDictionary<T>(this Type type)
        {
            return type.GetProperties()                       
                       .ToDictionary(p => p.Name.ToLowerInvariant());
        }

        public static PropertyInfo GetMemberAsPropertyInfo<T>(this Expression<Func<T, object>> expression)
           where T : class           
        {
            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression = lambda.Body as MemberExpression;
            
            return memberExpression?.Member as PropertyInfo;
        }
    }
}
