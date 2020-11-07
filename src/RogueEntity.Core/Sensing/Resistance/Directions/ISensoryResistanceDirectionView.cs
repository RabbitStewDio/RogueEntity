using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Resistance.Directions
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter", MessageId = nameof(TSense), Justification = "Used as instance qualifier")]
    public interface ISensoryResistanceDirectionView<TSense> : IReadOnlyDynamicDataView3D<DirectionalityInformation>
    {
    }
}