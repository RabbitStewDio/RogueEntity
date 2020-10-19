using EnTTSharp.Entities;
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
                                                                         in SingleLevelSenseDirectionMapData<TTargetSense> brightnessMap,
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
            level.ProcessOmnidirectional(blitter, senseMap, v.GetComponent(k, out SenseReceptorDirtyFlag<TTargetSense> _));
            v.WriteBack(k, new SingleLevelSenseDirectionMapData<TemperatureSense>(state.LastPosition.GridZ, senseMap));
            
            SenseReceptors.CopyReceptorFieldOfView(senseMap, state.LastPosition, perceptionFoV, senseMap);
        }
        
    }
}