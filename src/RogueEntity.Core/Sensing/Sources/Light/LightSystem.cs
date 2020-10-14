using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    /// <summary>
    ///   Performs light calculations by collecting all active lights and aggregating their affected areas into a global brightness map.
    /// </summary>
    public class LightSystem : SenseSystemBase<VisionSense, LightSourceDefinition>, IBrightnessMap
    {

        public LightSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                           [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                           [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm,
                           ISenseDataBlitter blitterFactory = null): base(senseProperties, senseCacheProvider, sensePropagationAlgorithm, blitterFactory)
        {
        }

        public bool TryGetLightData(int z, out ISenseDataView brightnessMap)
        {
            return TryGetSenseData(z, out brightnessMap);
        }

    }
}