using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Wayless.Core;

namespace Wayless
{
    public class SetRuleBuilder<TDestination, TSource>
        : ISetRuleBuilder<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        /// <summary>
        /// New instance of set rule builder. By default not MatchMaker and ExpressionBuilder
        /// are assigned. Use the "UseDefault()" extension method to assign built-in types
        /// </summary>
        public SetRuleBuilder()
        {
            IsMapUpToDate = false;

            FieldExpressions = new Dictionary<string, Expression>();
            FieldSkips = new List<string>();

            DestinationFields = typeof(TDestination).ToMemberInfoDictionary(true);
            SourceFields = typeof(TSource).ToMemberInfoDictionary();
        }

        public IExpressionBuilder ExpressionBuilder { get; set; }
        public IMatchMaker MatchMaker { get; set; }
        public IDictionary<string, MemberInfo> DestinationFields { get; }
        public IDictionary<string, MemberInfo> SourceFields { get; }
        public IDictionary<string, Expression> FieldExpressions { get; }
        public IList<string> FieldSkips { get; }        
        public bool IsMapUpToDate { get; private set; }        
        public bool AutoMatchMembers { get; set; } = true;

        /// <summary>
        /// Create a mapping rule between destination property and 
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="sourceExpression">Expression for property to be read in source type</param>
        /// <returns>Current instance of WaylessMap</returns>
        public ISetRuleBuilder<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression
                                                      , Expression<Func<TSource, object>> sourceExpression)
        {
            FieldMap(destinationExpression, sourceExpression, null);

            return this;
        }

        /// <summary>
        /// Create a mapping rule between destination property and 
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="sourceExpression">Expression for property to be read in source type</param>
        /// <param name="condition">Conditition to be evaluated for determening if values should be mapped</param>
        /// <returns>Current instance of WaylessMap</returns>
        public ISetRuleBuilder<TDestination, TSource> FieldMap(Expression<Func<TDestination, object>> destinationExpression
                                                      , Expression<Func<TSource, object>> sourceExpression
                                                      , Expression<Func<TSource, bool>> condition)
        {
            var destination = GetMemberName(destinationExpression);

            if (!FieldSkips.Contains(destination))
            {
                var expression = ExpressionBuilder.GetMapExpression(destinationExpression, sourceExpression, condition);

                RegisterFieldExpression(destination, expression);

                IsMapUpToDate = false;
            }

            return this;
        }

        /// <summary>
        /// Create mapping rule to explicitly assign value to destination property
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="value">Value to assign</param>
        /// <returns>Current instance of WaylessMap</returns>
        public ISetRuleBuilder<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression, object value)
        {
            FieldSet(destinationExpression, value, null);
            return this;
        }

        /// <summary>
        /// Create mapping rule to explicitly assign value to destination property
        /// </summary>
        /// <param name="destinationExpression">Expression for property to be set in destination type</param>
        /// <param name="value">Value to assign</param>
        /// <param name="condition">Conditition to be evaluated for determening if values should be set</param>
        /// <returns>Current instance of WaylessMap</returns>
        public ISetRuleBuilder<TDestination, TSource> FieldSet(Expression<Func<TDestination, object>> destinationExpression
                                                      , object value
                                                      , Expression<Func<TSource, bool>> setCondition)
        {
            var destination = GetMemberName(destinationExpression);
            if (!FieldSkips.Contains(destination))
            {
                var expression = ExpressionBuilder.GetMapExressionForExplicitSet(destinationExpression, value, setCondition);

                RegisterFieldExpression(destination, expression);

                IsMapUpToDate = false;
            }
            return this;
        }
        /// <summary>
        /// Create mapping rule for properties to be skipped in destination type
        /// </summary>
        /// <param name="ignoreAtDestinationExpression">Expression for property to be skipped in destination type</param>
        /// <returns>Current instance of WaylessMap</returns>
        public ISetRuleBuilder<TDestination, TSource> FieldSkip(Expression<Func<TDestination, object>> skipperName)
        {
            var ignore = GetMemberName(skipperName);

            if (!FieldSkips.Contains(ignore))
            {
                FieldSkips.Add(ignore);
            }

            if (FieldExpressions.ContainsKey(ignore))
            {
                FieldExpressions.Remove(ignore);
            }

            IsMapUpToDate = false;
            return this;
        }

        private static string GetMemberName<T>(Expression<Func<T, object>> expression)
        {
            var propertyInfo = expression.GetMemberInfo();

            return propertyInfo.Name;
        }

        private ISetRuleBuilder<TDestination, TSource> RegisterFieldExpression(string destination, Expression expression)
        {
            if (FieldExpressions.ContainsKey(destination))
            {
                FieldExpressions[destination] = expression;
            }
            else
            {
                FieldExpressions.Add(destination, expression);
            }

            IsMapUpToDate = false;

            return this;
        }
    }
}
