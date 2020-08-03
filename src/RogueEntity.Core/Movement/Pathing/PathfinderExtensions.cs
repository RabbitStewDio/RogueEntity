using GoRogue;
using GoRogue.Pathing;
using RogueEntity.Core.Infrastructure.Positioning.Grid;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement.Pathing
{
    public static class PathfinderExtensions
    {
        public static Path<EntityGridPosition> ToGridPath(this Path<Coord> coord, EntityGridPosition origin)
        {
            var p = new EntityGridPosition[coord.Count];
            var originLayerId = origin.LayerId;
            var originGridZ = origin.GridZ;

            for (int i = 0; i < coord.Count; i += 1)
            {
                var c = coord[i];
                p[i] = EntityGridPosition.OfRaw(originLayerId, c.X, c.Y, originGridZ);
            }

            return new Path<EntityGridPosition>(p, 0);
        }
    }
}