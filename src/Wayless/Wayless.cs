using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Wayless.Core;

namespace Wayless
{
    public sealed class Wayless<TDestination, TSource>
        : IWayless<TDestination, TSource>
        where TDestination : class
        where TSource : class
    {
        /// Type activator. Using static compiled expression for improved performance
        private static readonly Func<TDestination> _createDestinationInstance = Helpers.LambdaCreateInstance<TDestination>();

        private Action<TDestination, TSource> _map;

        public Wayless(ISetRuleBuilder<TDestination, TSource> setRuleBuilder)
        {
            if (setRuleBuilder.ExpressionBuilder == null)
            {
                throw new NullReferenceException("SetRuleBuilder.ExpressionBuilder");
            }

            if (!setRuleBuilder.IsFinalized)
            {
                setRuleBuilder.FinalizeRules();
            }

            _map = CompileMap(setRuleBuilder);
        }

        /// <summary>
        /// Type to map from
        /// </summary>
        public Type SourceType => typeof(TSource);

        /// <summary>
        /// Type to map to
        /// </summary>
        public Type DestinationType => typeof(TDestination);

        /// <summary>
        /// Apply mapping rules to  existing instance of object
        /// </summary>
        /// <param name="destinationObject">Object to apply mapping rules to</param>
        /// <param name="sourceObject">Object to read values from</param>
        public void Map(TDestination destinationObject, TSource sourceObject)
        {
            if (destinationObject == null || sourceObject == null)
            {
                return;
            }

            _map(destinationObject, sourceObject);
        }

        /// <summary>
        /// Apply mapping using a collection of source objects. Each object in the 
        /// source list will be mapped to a corresponding object in the output list
        /// </summary>
        /// <param name="sourceList">List of object sto map</param>
        /// <param name="constructorParameters">Constructor parameters, if any, to be used when 
        /// creating an instance of the destination object</param>
        /// <returns>Collection of mapped objects</returns>
        public IEnumerable<TDestination> Map(IEnumerable<TSource> sourceList)
        {
            if (sourceList == null)
            {
                return null;
            }

            IList<TDestination> mappedObjects = new List<TDestination>();
            foreach (var sourceObject in sourceList)
            {
                mappedObjects.Add(Map(sourceObject));
            }

            return mappedObjects;
        }

        /// <summary>
        /// Apply mapping rules in source object
        /// </summary>
        /// <param name="sourceObject">Object to read avalues from</param>
        /// <param name="constructorParameters">Parameters passed to destination object constructor</param>
        /// <returns>Mapped object</returns>
        public TDestination Map(TSource sourceObject)
        {
            if (sourceObject == null)
            {
                return null;
            }

            TDestination destinationObject = _createDestinationInstance();

            _map(destinationObject, sourceObject);

            return destinationObject;
        }


        private static Action<TDestination, TSource> CompileMap(ISetRuleBuilder<TDestination, TSource> setRuleBuilder)
        {
            return setRuleBuilder.ExpressionBuilder.CompileExpressionMap<TDestination, TSource>(setRuleBuilder.FieldExpressions.Values);
        }
    }
}
