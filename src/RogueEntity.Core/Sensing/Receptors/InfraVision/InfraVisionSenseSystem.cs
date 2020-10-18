using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public class InfraVisionSenseSystem: SenseReceptorSystemBase<TemperatureSense, TemperatureSense>
    {
        [NotNull] readonly ISenseDataBlitter blitter;

        public InfraVisionSenseSystem([NotNull] Lazy<ISensePropertiesSource> senseProperties,
                                      [NotNull] Lazy<ISenseStateCacheProvider> senseCacheProvider,
                                      [NotNull] ISensePhysics physics,
                                      [NotNull] ISensePropagationAlgorithm sensePropagationAlgorithm,
                                      [NotNull] ISenseDataBlitter blitter) : base(senseProperties, senseCacheProvider, physics, sensePropagationAlgorithm)
        {
            this.blitter = blitter ?? throw new ArgumentNullException(nameof(blitter));
        }
        
        public void CopySenseSourcesToVisionField<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                         TGameContext context,
                                                                         TItemId k,
                                                                         in SingleLevelBrightnessMap brightnessMap,
                                                                         in SensoryReceptorState<TemperatureSense> state)
            where TItemId : IEntityKey
        {
            if (!state.LastPosition.IsInvalid && TryGetLevel(state.LastPosition.GridZ, out var level))
            {
                var senseMap = brightnessMap.SenseMap;
                level.ProcessOmnidirectional(blitter, senseMap, v.GetComponent(k, out SenseReceptorDirtyFlag<TemperatureSense> _));
                brightnessMap.Z = state.LastPosition.GridZ;
            }
        }

    }
}