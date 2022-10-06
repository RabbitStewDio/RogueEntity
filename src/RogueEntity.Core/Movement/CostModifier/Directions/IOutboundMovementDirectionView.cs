using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.CostModifier.Directions
{
    /// <summary>
    ///   A data view that records whether an entity can exit a given cell from a given direction.
    ///   To detect whether an entity can move from a given cell A to an adjacent cell B, query
    ///   the coordinates of cell B with the direction of A -> B.
    ///
    ///   This view is used when attempting to move from a given source cell towards a target
    ///   cell. Used by the forward seeking pathfinder.
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter", MessageId = nameof(TMovementMode), Justification = "Used as instance qualifier")]
    public interface IOutboundMovementDirectionView<TMovementMode>
    {
        IAggregateDynamicDataView3D<DirectionalityInformation> ResultView { get; }
    }
}