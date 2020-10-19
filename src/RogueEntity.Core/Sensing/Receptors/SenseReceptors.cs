using GoRogue;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Receptors
{
    public static class SenseReceptors
    {
        public static readonly EntityRole SenseReceptorRole = new EntityRole("Role.Core.Senses.Receptor");

        public static void CopyReceptorFieldOfView(SenseDataMap dest,
                                                   Position lastPosition,
                                                   SenseSourceData sourceData,
                                                   ISenseDataView lights)
        {
            var bounds = new Rectangle(new Coord(lastPosition.GridX, lastPosition.GridY), sourceData.Radius, sourceData.Radius);
            foreach (var d in bounds)
            {
                if (lights.TryQuery(d.X, d.Y, out var intensity, out var dir) &&
                    sourceData.TryQuery(d.X, d.Y, out var perceptionStr, out var perceptionDir) &&
                    perceptionStr > 0)
                {
                    dest.TryStore(d.X, d.Y, perceptionStr * intensity, dir.Merge(perceptionDir));
                }
            }
        }
    }
}