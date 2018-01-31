using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Wayless
{
    internal static class Helpers
    {
        public static IDictionary<string, MemberInfo> ToMemberInfoDictionary(this Type type, bool readOnly = false)
        {
            var members = type.GetFields().Cast<MemberInfo>().ToList();

            members.AddRange(type.GetProperties().Cast<MemberInfo>());

            return members.ToDictionary(p => p.Name);
        }

        public static MemberInfo GetMemberInfo<T>(this Expression<Func<T, object>> expression)
           where T : class           
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

            return memberExpression?.Member as MemberInfo;
        }

        public static Func<object> CreateInstanceLambda(Type type)
        {            
            var instanceExpression = Expression.Lambda<Func<object>>(Expression.New(type
                                                               .GetConstructor(Type.EmptyTypes)));
            
            return instanceExpression.Compile();
        }

        public static Func<T> LambdaCreateInstance<T>()
        {
            var instanceCreator = Expression.Lambda<Func<T>>(Expression.New(typeof(T)
                                                               .GetConstructor(Type.EmptyTypes)))
                                                               .Compile();

            return instanceCreator;
        }

        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
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
