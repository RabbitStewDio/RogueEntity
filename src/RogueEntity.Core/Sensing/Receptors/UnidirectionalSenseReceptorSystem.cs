using EnTTSharp.Entities;
using RogueEntity.Core.Sensing.Common.Blitter;

namespace RogueEntity.Core.Sensing.Receptors
{
    public class UnidirectionalSenseReceptorSystem<TTargetSense, TSourceSense>
        where TTargetSense : ISense
        where TSourceSense : ISense
    {
        readonly IDirectionalSenseBlitter blitter;
        readonly SenseReceptorSystem<TTargetSense, TSourceSense> receptorSystem;

        public UnidirectionalSenseReceptorSystem(SenseReceptorSystem<TTargetSense, TSourceSense> receptorSystem, 
                                                 IDirectionalSenseBlitter blitter)
        {
            this.receptorSystem = receptorSystem;
            this.blitter = blitter ?? new DefaultDirectionalSenseBlitter();
        }

        public void CopySenseSourcesToVisionField<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                         TGameContext context,
                                                                         TItemId k,
                                                                         in SingleLevelSenseDirectionMapData<TTargetSense, TSourceSense> brightnessMap,
                                                                         in SensoryReceptorState<TTargetSense> state)
            where TItemId : IEntityKey
        {
            if (state.LastPosition.IsInvalid || 
                !receptorSystem.TryGetLevel(state.LastPosition.GridZ, out var level) ||
                !state.SenseSource.TryGetValue(out var perceptionFoV))
            {
                return;
            }

            var senseMap = brightnessMap.SenseMap;
            level.ProcessDirectional(blitter, state.LastPosition, senseMap, v.GetComponent(k, out SenseReceptorDirtyFlag<TTargetSense> _));
            brightnessMap.WithLevel(state.LastPosition.GridZ);
            
            SenseReceptors.CopyReceptorFieldOfView(senseMap, state.LastPosition, perceptionFoV, senseMap);
        }

    }
}