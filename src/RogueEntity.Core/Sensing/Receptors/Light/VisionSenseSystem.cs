using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    /// <summary>
    ///   Computes a local character's vision area. The vision area is all the space
    ///   a character can see under perfect light conditions. 
    /// </summary>
    /// <remarks>
    ///   To calculate the visibility of a objects for a given character, combine the
    ///   environment's brightness map (see sense-sources module) with this vision area.
    /// </remarks>
    public class VisionSenseSystem<TSourceSense> : SenseReceptorSystemBase<VisionSense, TSourceSense>
    {
        public VisionSenseSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                                 [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                                 [NotNull] ISensePhysics physics,
                                 [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm) : base(senseProperties, senseCacheProvider, physics, sensePropagationAlgorithm)
        {
        }

        public void CopySenseSourcesToVisionField<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                         TGameContext context,
                                                                         TItemId k,
                                                                         in SingleLevelBrightnessMap brightnessMap,
                                                                         in SensoryReceptorState<VisionSense> state,
                                                                         in SenseReceptorDirtyFlag<VisionSense> dirty)
            where TItemId : IEntityKey
        {
            if (!state.LastPosition.IsInvalid && TryGetLevel(state.LastPosition.GridZ, out var level))
            {
                var senseMap = brightnessMap.SenseMap;
                level.Process(senseMap);
                brightnessMap.Z = state.LastPosition.GridZ;
            }
        }

    }
}