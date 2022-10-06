using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Movement.CostModifier.Directions
{
    /// <summary>
    ///   A data view that records whether an entity can enter a given cell from a given direction.
    ///   To detect whether an entity can move from a given cell A to an adjacent cell B, query
    ///   the coordinates of cell B with the direction of A -> B.
    ///
    ///   This view is used when searching from an target cell towards source cells, as used
    ///   by the goal seeker.
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter", MessageId = nameof(TMovementMode), Justification = "Used as instance qualifier")]
    public interface IInboundMovementDirectionView<TMovementMode>
    {
        IAggregateDynamicDataView3D<DirectionalityInformation> ResultView { get; }
    }
}
