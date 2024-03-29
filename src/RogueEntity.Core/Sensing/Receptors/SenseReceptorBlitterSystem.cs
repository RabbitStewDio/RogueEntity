using System;
using EnTTSharp.Entities;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Receptors
{
    public class SenseReceptorBlitterSystem<TTargetSense, TSourceSense>
        where TTargetSense : ISense
        where TSourceSense : ISense
    {
        readonly ISenseReceptorBlitter receptorBlitter;
        readonly SenseReceptorSystem<TTargetSense, TSourceSense> receptorSystem;

        public SenseReceptorBlitterSystem(SenseReceptorSystem<TTargetSense, TSourceSense> receptorSystem,
                                          ISenseReceptorBlitter receptorBlitter)
        {
            this.receptorSystem = receptorSystem;
            this.receptorBlitter = receptorBlitter ?? throw new ArgumentNullException(nameof(receptorBlitter));
        }

        public void CopySenseSourcesToVisionField<TItemId>(IEntityViewControl<TItemId> v,
                                                                         TItemId k,
                                                                         in SensoryReceptorState<TTargetSense, TSourceSense> receptorState,
                                                                         ref SingleLevelSenseDirectionMapData<TTargetSense, TSourceSense> receptorSenseMap)
            where TItemId : struct, IEntityKey
        {
            if (receptorState.LastPosition.IsInvalid ||
                !receptorSystem.TryGetLevel(receptorState.LastPosition.GridZ, out var level) ||
                !receptorState.SenseSource.TryGetValue(out var perceptionFoV))
            {
                receptorSenseMap = receptorSenseMap.WithDisabledState();
                return;
            }

            var senseBoundaries = perceptionFoV.Bounds.WithCenter(new GridPosition2D(receptorState.LastPosition.GridX, receptorState.LastPosition.GridY));
            var senseMap = receptorSenseMap.SenseMap;
            level.ProcessDirectional(receptorBlitter, receptorState.LastPosition, senseMap, senseBoundaries, v.GetComponent(k, out SenseReceptorDirtyFlag<TTargetSense, TSourceSense> _));
            receptorSenseMap = receptorSenseMap.WithLevel(receptorState.LastPosition.GridZ);

            SenseReceptors.ApplyReceptorFieldOfView(senseMap, receptorState.LastPosition, receptorState.LastIntensity, perceptionFoV, senseMap);
        }
    }
}