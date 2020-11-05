using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Sensing.Receptors
{
    public static class SenseReceptors
    {
        public static readonly EntityRole SenseReceptorRole = new EntityRole("Role.Core.Senses.Receptor");

        /// <summary>
        ///    Filters the raw sense data by the receptor's field of view. This only needs to adjust
        ///    the intensities.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="lastPosition"></param>
        /// <param name="maxPerception"></param>
        /// <param name="receptorFieldOfView"></param>
        /// <param name="receptorSenseMap"></param>
        public static void ApplyReceptorFieldOfView(SenseDataMap dest,
                                                   Position lastPosition,
                                                   float maxPerception,
                                                   SenseSourceData receptorFieldOfView,
                                                   IDynamicSenseDataView2D receptorSenseMap)
        {
            var bounds = receptorFieldOfView.Bounds;
            foreach (var (x, y) in bounds.Contents)
            {
                var mapX = x + lastPosition.GridX;
                var mapY = y + lastPosition.GridY;
                
                if (receptorSenseMap.TryQuery(mapX, mapY, out var intensity, out var senseDirection) &&
                    receptorFieldOfView.TryQuery(x, y, out var perceptionStr, out _) &&
                    perceptionStr > 0)
                {
                    var targetIntensity = intensity * (perceptionStr / maxPerception);
                    dest.TryStore(mapX, mapY, targetIntensity, senseDirection);
                }
                else
                {
                    dest.TryStore(mapX, mapY, 0, default);
                }
            }
        }
    }
}