using System;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Map.Light
{
    public class LightMapSystem : SenseMappingSystemBase<VisionSense, VisionSense, LightSourceDefinition>, IBrightnessMap
    {
        public LightMapSystem([NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                              [NotNull] Lazy<ITimeSource> timeSource,
                              ISenseDataBlitter blitterFactory = null) : base(senseCacheProvider, timeSource, blitterFactory)
        {
        }

        public bool TryGetLightIntensity(int z, out ISenseDataView brightnessMap)
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