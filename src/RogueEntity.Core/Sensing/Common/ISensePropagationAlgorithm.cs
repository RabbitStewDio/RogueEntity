using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common
{
    public interface ISensePropagationAlgorithm
    {
        SenseSourceData Calculate<TResistanceMap>(SenseSourceDefinition sense,
                                                  Position2D position, 
                                                  TResistanceMap resistanceMap,
                                                  SenseSourceData data = null)
            where TResistanceMap : IReadOnlyView2D<float>;
    }
}