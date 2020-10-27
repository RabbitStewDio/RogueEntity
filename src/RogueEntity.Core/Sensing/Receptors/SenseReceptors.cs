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
                                                   float maxPerception,
                                                   SenseSourceData sourceData,
                                                   ISenseDataView lights)
        {
            var bounds = sourceData.Bounds;
            foreach (var (x, y) in bounds.Contents)
            {
                var mapX = x + lastPosition.GridX;
                var mapY = y + lastPosition.GridY;
                
                if (lights.TryQuery(mapX, mapY, out var intensity, out var dir) &&
                    sourceData.TryQuery(x, y, out var perceptionStr, out var perceptionDir) &&
                    perceptionStr > 0)
                {
                    dest.TryStore(mapX, mapY, intensity * (perceptionStr / maxPerception), dir.Merge(perceptionDir));
                }
                else
                {
                    dest.TryStore(mapX, mapY, 0, default);
                }
            }
        }
    }
}