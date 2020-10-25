using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common
{
    public interface ISensePropagationAlgorithm
    {
        SenseSourceData Calculate<TResistanceMap>(in SenseSourceDefinition sense,
                                                  float intensity,
                                                  in Position2D position, 
                                                  in TResistanceMap resistanceMap,
                                                  SenseSourceData data = null)
            where TResistanceMap : IReadOnlyView2D<float>;
    }
}