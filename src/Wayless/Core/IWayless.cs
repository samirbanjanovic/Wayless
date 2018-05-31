using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Wayless.Core
{
    public interface IWayless<TDestination, TSource>        
        where TDestination : class
        where TSource : class
    {

        Type DestinationType { get; }
        Type SourceType { get; }
        IEnumerable<TDestination> Map(IEnumerable<TSource> sourceList);
        void Map(TDestination destinationObject, TSource sourceObject);
        TDestination Map(TSource sourceObject);        
    }
}