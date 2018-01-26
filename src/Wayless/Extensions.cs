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
        public static IDictionary<string, MemberInfo> GetMemberInfo(this Type type)
        {
            var members = type.GetFields().Cast<MemberInfo>().ToList();
            members.AddRange(type.GetProperties().Cast<MemberInfo>());

            return members.ToDictionary(p => p.Name.ToLowerInvariant());
        }

        public static MemberInfo GetMemberInfo<T>(this Expression<Func<T, object>> expression)
           where T : class           
        {
            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression = lambda.Body as MemberExpression;
            
            return memberExpression?.Member as MemberInfo;
        }

        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                //case MemberTypes.Event:
                //    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                //case MemberTypes.Method:
                //    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }
    }
}
