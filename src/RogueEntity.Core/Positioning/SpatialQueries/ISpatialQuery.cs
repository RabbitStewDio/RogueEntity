using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public delegate void ReceiveSpatialQueryResult<TEntityId, TComponent>(in SpatialQueryResult<TEntityId, TComponent> c);

    public interface ISpatialQuery<TEntityId>
        where TEntityId : IEntityKey
    {
        /// <summary>
        ///    Returns all entities within a given 2D plane denoted by the position's z-index. 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <param name="d"></param>
        /// <param name="buffer"></param>
        /// <typeparam name="TComponent"></typeparam>
        List<SpatialQueryResult<TEntityId, TComponent>> Query2D<TComponent>(in Position pos,
                                                                            float distance = 1,
                                                                            DistanceCalculation d = DistanceCalculation.Euclid,
                                                                            List<SpatialQueryResult<TEntityId, TComponent>> buffer = null);

        /// <summary>
        ///    Returns all entities within a given 3D volume around the given position. 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <param name="d"></param>
        /// <param name="buffer"></param>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        List<SpatialQueryResult<TEntityId, TComponent>> Query3D<TComponent>(in Position pos,
                                                                            float distance = 1,
                                                                            DistanceCalculation d = DistanceCalculation.Euclid,
                                                                            List<SpatialQueryResult<TEntityId, TComponent>> buffer = null);
    }
}
