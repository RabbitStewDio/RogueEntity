using System;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Map.Light
{
    public class LightMapSystem : SenseMappingSystemBase<VisionSense, VisionSense, LightSourceDefinition>, IBrightnessMap
    {
        public LightMapSystem([NotNull] Lazy<ITimeSource> timeSource,
                              ISenseMapDataBlitter blitterFactory) : base(timeSource, blitterFactory)
        {
        }

        public bool TryGetLightIntensity(int z, out IDynamicSenseDataView2D brightnessMap)
        {
            return TryGetSenseData(z, out brightnessMap);
        }

        public bool TryGetLightColors(int z, out IReadOnlyView2D<Color> colorMap)
        {
            colorMap = default;
            return false;
        }
    }
}