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
        /// <param name="receiver"></param>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <param name="d"></param>
        /// <typeparam name="TComponent"></typeparam>
        void Query2D<TComponent>(ReceiveSpatialQueryResult<TEntityId, TComponent> receiver,
                                 in Position pos,
                                 float distance = 1,
                                 DistanceCalculation d = DistanceCalculation.Euclid);

        /// <summary>
        ///    Returns all entities within a given 3D volume around the given position. 
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <param name="d"></param>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        void Query3D<TComponent>(ReceiveSpatialQueryResult<TEntityId, TComponent> receiver,
                                 in Position pos,
                                 float distance = 1,
                                 DistanceCalculation d = DistanceCalculation.Euclid);
    }
}