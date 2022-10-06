using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Resistance.Directions
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter", MessageId = nameof(TSense), Justification = "Used as instance qualifier")]
    public interface ISensoryResistanceDirectionView<TSense>
    {
        IAggregateDynamicDataView3D<DirectionalityInformation> ResultView { get; }
    }
}