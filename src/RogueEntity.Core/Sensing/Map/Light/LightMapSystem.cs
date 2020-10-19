using System;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Map.Light
{
    public class LightMapSystem : SenseMappingSystemBase<VisionSense, LightSourceDefinition>, IBrightnessMap
    {
        public LightMapSystem([NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                              ISenseDataBlitter blitterFactory = null) : base(senseCacheProvider, blitterFactory)
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