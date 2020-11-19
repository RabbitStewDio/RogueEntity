using RogueEntity.Core.Directionality;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Common
{
    public interface ISensePropagationAlgorithm
    {
        SenseSourceData Calculate<TResistanceMap>(in SenseSourceDefinition sense,
                                                  float intensity,
                                                  in Position2D position,
                                                  in TResistanceMap resistanceMap,
                                                  IReadOnlyView2D<DirectionalityInformation> directionalityView,
                                                  SenseSourceData data = null)
            where TResistanceMap : IReadOnlyView2D<float>;
    }
}