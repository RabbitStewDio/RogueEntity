using RogueEntity.Core.Directionality;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Common
{
    public interface ISensePropagationAlgorithm
    {
        SenseSourceData Calculate(in SenseSourceDefinition sense,
                                  float intensity,
                                  in Position2D position,
                                  IReadOnlyDynamicDataView2D<float> resistanceMap,
                                  IReadOnlyDynamicDataView2D<DirectionalityInformation> directionalityView,
                                  SenseSourceData data = null);
    }
}