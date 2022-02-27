using System;
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
        IDictionary<string, MemberInfo> DestinationFields { get; }
        IDictionary<string, MemberInfo> SourceFields { get; }
        IList<string> FieldSkips { get; }
        IDictionary<string, Expression> FieldExpressions { get; }
        IExpressionBuilder ExpressionBuilder { get; set; }
        IMatchMaker MatchMaker { get; set; }
        bool AutoMatchMembers { get; set; }
        bool IsFinalized { get; }

        ISetRuleBuilder<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression);
        ISetRuleBuilder<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<TSource, object>> sourceExpression, Expression<Func<TSource, bool>> mapOnCondition);
        ISetRuleBuilder<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object fieldValue);
        ISetRuleBuilder<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object value, Expression<Func<TSource, bool>> setCondition);
        ISetRuleBuilder<TDestination, TSource> FieldSet<T>(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<T>> fieldValue);
        ISetRuleBuilder<TDestination, TSource> FieldSet<T>(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<T>> fieldValue, Expression<Func<TSource, bool>> setCondition);
        ISetRuleBuilder<TDestination, TSource> FieldSkip(Expression<Func<TDestination, object>> ignoreAtDestinationExpression);

        void FinalizeRules();

    }
}
