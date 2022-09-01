using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public interface ISpatialQuery<TEntityId>
        where TEntityId : IEntityKey
    {
        /// <summary>
        ///    Returns all entities within a given 3D box volume around the given position. 
        /// </summary>
        BufferList<SpatialQueryResult<TEntityId, TComponent>> QueryBox<TComponent>(in Rectangle3D queryRegion,
                                                                                   BufferList<SpatialQueryResult<TEntityId, TComponent>> buffer = null);

        /// <summary>
        ///    Returns all entities within a given 3D sphere volume around the given position. 
        /// </summary>
        BufferList<SpatialQueryResult<TEntityId, TComponent>> QuerySphere<TComponent>(in Position pos,
                                                                                      float distance = 1,
                                                                                      DistanceCalculation d = DistanceCalculation.Euclid,
                                                                                      BufferList<SpatialQueryResult<TEntityId, TComponent>> buffer = null);
    }
}
