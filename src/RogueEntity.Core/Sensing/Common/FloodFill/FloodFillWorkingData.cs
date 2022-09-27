using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Common.FloodFill
{
    public class FloodFillWorkingData
    {
        public readonly FloodFillDijkstraMap ResultMap;

        public FloodFillWorkingData(in SenseSourceDefinition sense,
                                    float intensity,
                                    in Position2D origin,
                                    ISensePhysics sensePhysics,
                                    IReadOnlyDynamicDataView2D<float> resistanceMap,
                                    IReadOnlyDynamicDataView2D<DirectionalityInformation> directionalityView)
        {
            ResultMap = FloodFillDijkstraMap.Create(in sense, intensity, in origin, sensePhysics, resistanceMap, directionalityView);
        }

        public void Configure(in SenseSourceDefinition sense,
                              float intensity,
                              in Position2D origin,
                              ISensePhysics sensePhysics,
                              IReadOnlyDynamicDataView2D<float> resistanceMap,
                              IReadOnlyDynamicDataView2D<DirectionalityInformation> directionalityView)
        {
            ResultMap.Configure(in sense, intensity, in origin, sensePhysics, resistanceMap, directionalityView);
        }
    }
}