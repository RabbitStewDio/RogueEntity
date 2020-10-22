using GoRogue;
using GoRogue.Pathing;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;

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

        public static float Calculate<TPosition>(this DistanceCalculation calc,
                                                 in TPosition posA,
                                                 in TPosition posB) where TPosition: IPosition
        {
            return calc.Calculate(posA.GridX, posA.GridY, posA.GridZ, posB.GridX, posB.GridY, posB.GridZ);
        }
    }
}