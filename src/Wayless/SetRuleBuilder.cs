using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool AutoMatchMembers { get; set; } = true;

        public bool IsFinalized { get; private set; }
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
                IsFinalized = false;
                var expression = ExpressionBuilder.GetMapExpression(destinationExpression, sourceExpression, condition);

                RegisterFieldExpression(destination, expression);
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

        public ISetRuleBuilder<TDestination, TSource> FieldSet<T>(Expression<Func<TDestination, object>> destinationExpression, Expression<Func<T>> value)
        {
            FieldSet(destinationExpression, value, null);
            return this;
        }

        public ISetRuleBuilder<TDestination, TSource> FieldSet<T>(Expression<Func<TDestination, object>> destinationExpression
                                                                , Expression<Func<T>> value
                                                                , Expression<Func<TSource, bool>> setCondition)
        {
            FieldSet(destinationExpression, (object)value, setCondition);
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
                IsFinalized = false;
                var expression = ExpressionBuilder.GetMapExressionForExplicitSet(destinationExpression, value, setCondition);

                RegisterFieldExpression(destination, expression);
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
                IsFinalized = false;
                FieldSkips.Add(ignore);
            }

            if (FieldExpressions.ContainsKey(ignore))
            {
                FieldExpressions.Remove(ignore);
            }

            return this;
        }

        /// <summary>
        /// Performs call to MatchMaker if AutoMatchMembres is True
        /// </summary>
        public void FinalizeRules()
        {
            if (AutoMatchMembers)
            {
                if (MatchMaker == null)
                {
                    throw new NullReferenceException(nameof(MatchMaker));
                }

                var unmappedDestinations = DestinationFields.Where(x => !FieldExpressions.Keys.Contains(x.Key)
                                                                      && !FieldSkips.Contains(x.Key))
                                                             .Select(x => x.Value)
                                                             .ToList();

                var matchedPairs = MatchMaker.FindMemberPairs(unmappedDestinations, SourceFields.Values);
                foreach (var pair in matchedPairs)
                {
                    var expression = ExpressionBuilder.GetMapExpression<TSource>(pair.DestinationMember, pair.SourceMember);
                    FieldExpressions.Add(pair.DestinationMember.Name, expression);
                }
            }
           
            IsFinalized = true;
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

            return this;
        }
    }
}
