using System;
using System.Collections.Generic;
using System.Text;
using Wayless.Core;

namespace Wayless
{
    public static class WaylessConfigurationBuilder
    {
        public static IWaylessConfiguration GetEmptyConfiguration()
        {
            return new WaylessConfiguration();
        }

        public static IWaylessConfiguration GetDefaultConfiguration(Type destinationType, Type sourcetype)
        {
            var configuration = GetEmptyConfiguration()
                                    .UseDefaultExpressionBuilder(destinationType, sourcetype)
                                    .UseDefaultMatchMaker();

            return configuration;
        }

        public static IWaylessConfiguration GetDefaultConfiguration<TDestination, TSource>()
        {
            return GetDefaultConfiguration(typeof(TDestination), typeof(TSource));
        }

        public static IWaylessConfiguration OmitAutoMatch(this IWaylessConfiguration waylessConfiguration)
        {
            waylessConfiguration.AutoMatchMembers = false;

            return waylessConfiguration;
        }

        public static IWaylessConfiguration UseDefaultExpressionBuilder(this IWaylessConfiguration waylessConfiguration, Type destinationType, Type sourceType)
        {
            waylessConfiguration.ExpressionBuilder = new ExpressionBuilder(destinationType, sourceType);

            return waylessConfiguration;
        }

        public static IWaylessConfiguration UseDefaultExpressionBuilder<TDestination, TSource>(this IWaylessConfiguration waylessConfiguration)
        {
            waylessConfiguration.ExpressionBuilder = new ExpressionBuilder(typeof(TDestination), typeof(TSource));

            return waylessConfiguration;
        }

        public static IWaylessConfiguration UseDefaultMatchMaker(this IWaylessConfiguration waylessConfiguration)
        {
            waylessConfiguration.MatchMaker = new MatchMaker();

            return waylessConfiguration;
        }


    }
}
