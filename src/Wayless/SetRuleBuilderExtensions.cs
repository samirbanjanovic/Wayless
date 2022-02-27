﻿using System;
using System.Collections.Generic;
using System.Text;
using Wayless.Core;
using Wayless.PairMatching;

namespace Wayless
{
    public static class SetRuleBuilderExtensions
    {
        public static ISetRuleBuilder<TDestination, TSource> UseDefaults<TDestination, TSource>(this ISetRuleBuilder<TDestination, TSource> setRuleBuilder)
           where TDestination : class
           where TSource : class
        {
            setRuleBuilder.UseDefaultExpressionBuilder()
                          .UseDefaultMatchMaker();

            return setRuleBuilder;
        }

        public static ISetRuleBuilder<TDestination, TSource> UseDefaultExpressionBuilder<TDestination, TSource>(this ISetRuleBuilder<TDestination, TSource> setRuleBuilder)
            where TDestination : class
            where TSource : class
        {
            setRuleBuilder.ExpressionBuilder = new ExpressionBuilder(typeof(TDestination), typeof(TSource));

            return setRuleBuilder;
        }

        public static ISetRuleBuilder<TDestination, TSource> UseDefaultMatchMaker<TDestination, TSource>(this ISetRuleBuilder<TDestination, TSource> setRuleBuilder)
           where TDestination : class
           where TSource : class
        {
            setRuleBuilder.AutoMatchMembers = true;
            setRuleBuilder.MatchMaker = new MatchMaker();

            return setRuleBuilder;
        }

        public static ISetRuleBuilder<TDestination, TSource> OmitMatchMaker<TDestination, TSource>(this ISetRuleBuilder<TDestination, TSource> setRuleBuilder)
           where TDestination : class
           where TSource : class
        {
            setRuleBuilder.AutoMatchMembers = false;

            return setRuleBuilder;
        }

        public static ISetRuleBuilder<TDestination, TSource> UseJsonMappingMatchMaker<TDestination, TSource>(this ISetRuleBuilder<TDestination, TSource> setRuleBuilder, string path)
            where TDestination : class
            where TSource : class
        {
            setRuleBuilder.AutoMatchMembers = true;
            setRuleBuilder.MatchMaker = new JsonFileMatchMaker(path);

            return setRuleBuilder;
        }
    }
}
