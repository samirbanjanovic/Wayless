﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Wayless.Core
{
    public interface ISetRuleBuilder<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        IWaylessConfiguration WaylessConfiguration { get; }

        IDictionary<string, MemberInfo> DestinationFields { get; }
        IDictionary<string, MemberInfo> SourceFields { get; }
        IList<string> FieldSkips { get; }
        IDictionary<string, Expression> FieldExpressions { get; }

        bool IsMapUpToDate { get; }

        ISetRuleBuilder<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression);
        ISetRuleBuilder<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression, Expression<Func<TSource, bool>> mapOnCondition);
        ISetRuleBuilder<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object fieldValue);
        ISetRuleBuilder<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object value, Expression<Func<TSource, bool>> setCondition);
        ISetRuleBuilder<TDestination, TSource> FieldSkip(Expression<Func<TDestination, object>> ignoreAtDestinationExpression);

    }
}
