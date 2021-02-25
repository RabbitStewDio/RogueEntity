using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Positioning.SpatialQueries
{
    public static class SpatialQueryExtensions
    {
        public static BufferList<SpatialQueryResult<TEntityId, TComponent>> Query2D<TEntityId, TComponent>(this ISpatialQuery<TEntityId> q,
                                                                                                           in Position pos,
                                                                                                           BufferList<SpatialQueryResult<TEntityId, TComponent>> buffer = null)
            where TEntityId : IEntityKey
        {
            return q.Query2D(pos, 1, DistanceCalculation.Euclid, buffer);
        }

        public static BufferList<SpatialQueryResult<TEntityId, TComponent>> Query2D<TEntityId, TComponent>(this ISpatialQuery<TEntityId> q,
                                                                                                           in Position pos,
                                                                                                           int radius,
                                                                                                           BufferList<SpatialQueryResult<TEntityId, TComponent>> buffer = null)
            where TEntityId : IEntityKey
        {
            return q.Query2D(pos, radius, DistanceCalculation.Euclid, buffer);
        }

        public static BufferList<SpatialQueryResult<TEntityId, TComponent>> Query2D<TEntityId, TComponent>(this ISpatialQuery<TEntityId> q,
                                                                                                           in Position pos,
                                                                                                           DistanceCalculation c,
                                                                                                           BufferList<SpatialQueryResult<TEntityId, TComponent>> buffer = null)
            where TEntityId : IEntityKey
        {
            return q.Query2D(pos, 1, c, buffer);
        }


        public static BufferList<SpatialQueryResult<TEntityId, TComponent>> Query3D<TEntityId, TComponent>(this ISpatialQuery<TEntityId> q,
                                                                                                           in Position pos,
                                                                                                           BufferList<SpatialQueryResult<TEntityId, TComponent>> buffer = null)
            where TEntityId : IEntityKey
        {
            return q.Query3D(pos, 1, DistanceCalculation.Euclid, buffer);
        }

        public static BufferList<SpatialQueryResult<TEntityId, TComponent>> Query3D<TEntityId, TComponent>(this ISpatialQuery<TEntityId> q,
                                                                                                           in Position pos,
                                                                                                           int radius,
                                                                                                           BufferList<SpatialQueryResult<TEntityId, TComponent>> buffer = null)
            where TEntityId : IEntityKey
        {
            return q.Query3D(pos, radius, DistanceCalculation.Euclid, buffer);
        }

        public static BufferList<SpatialQueryResult<TEntityId, TComponent>> Query3D<TEntityId, TComponent>(this ISpatialQuery<TEntityId> q,
                                                                                                           in Position pos,
                                                                                                           DistanceCalculation c,
                                                                                                           BufferList<SpatialQueryResult<TEntityId, TComponent>> buffer = null)
            where TEntityId : IEntityKey
        {
            return q.Query3D(pos, 1, c, buffer);
        }
    }
}
