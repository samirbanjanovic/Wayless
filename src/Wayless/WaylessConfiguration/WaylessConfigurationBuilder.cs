using System;
using System.Collections.Generic;
using System.Text;
using Wayless.Core;

namespace Wayless
{
    public static class WaylessConfigurationBuilder
    {
        public static IWaylessConfiguration EmptyConfiguration()
        {
            return new WaylessConfiguration();
        }

        public static IWaylessConfiguration DefaultConfiguration(Type destinationType, Type sourcetype)
        {
            var configuration = EmptyConfiguration()
                                    .UseDefaultExpressionBuilder(destinationType, sourcetype)
                                    .UseDefaultMatchMaker()
                                    .AutoMatchMembers();

            return configuration;
        }

        public static IWaylessConfiguration DefaultConfiguration<TDestination, TSource>()
        {
            return DefaultConfiguration(typeof(TDestination), typeof(TSource));
        }

        public static IWaylessConfiguration AutoMatchMembers(this IWaylessConfiguration waylessConfiguration)
        {
            waylessConfiguration.AutoMatchMembers = true;

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
