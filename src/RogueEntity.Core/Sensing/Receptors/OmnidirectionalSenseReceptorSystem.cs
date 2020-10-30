using EnTTSharp.Entities;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common.Blitter;

namespace RogueEntity.Core.Sensing.Receptors
{
    public class OmnidirectionalSenseReceptorSystem<TTargetSense, TSourceSense>
        where TTargetSense : ISense
        where TSourceSense : ISense
    {
        readonly ISenseDataBlitter blitter;
        readonly SenseReceptorSystem<TTargetSense, TSourceSense> receptorSystem;

        public OmnidirectionalSenseReceptorSystem(SenseReceptorSystem<TTargetSense, TSourceSense> receptorSystem, 
                                                  ISenseDataBlitter blitter)
        {
            this.receptorSystem = receptorSystem;
            this.blitter = blitter ?? new DefaultSenseDataBlitter();
        }

        public void CopySenseSourcesToVisionField<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                         TGameContext context,
                                                                         TItemId k,
                                                                         in SingleLevelSenseDirectionMapData<TTargetSense, TSourceSense> brightnessMap,
                                                                         in SensoryReceptorState<TTargetSense, TSourceSense> state)
            where TItemId : IEntityKey
        {
            if (state.LastPosition.IsInvalid || 
                !receptorSystem.TryGetLevel(state.LastPosition.GridZ, out var level) ||
                !state.SenseSource.TryGetValue(out var perceptionFoV))
            {
                v.WriteBack(k, brightnessMap.WithDisabledState());
                return;
            }

            var senseBoundaries = perceptionFoV.Bounds.WithCenter(new Position2D(state.LastPosition.GridX, state.LastPosition.GridY));
            var senseMap = brightnessMap.SenseMap;
            level.ProcessOmnidirectional(blitter, senseMap, senseBoundaries, v.GetComponent(k, out SenseReceptorDirtyFlag<TTargetSense, TSourceSense> _));
            v.WriteBack(k, brightnessMap.WithLevel(state.LastPosition.GridZ));
            
            SenseReceptors.CopyReceptorFieldOfView(senseMap, state.LastPosition, state.LastIntensity, perceptionFoV, senseMap);
        }
        
    }
}