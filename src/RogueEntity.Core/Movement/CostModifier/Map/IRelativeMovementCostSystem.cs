using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.GridProcessing.LayerAggregation;

namespace RogueEntity.Core.Movement.CostModifier.Map
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminator")]
    public interface IRelativeMovementCostSystem<TMovementType> : IAggregationLayerSystem<float>
    {
    }
}