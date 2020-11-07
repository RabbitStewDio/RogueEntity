using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.Resistance.Directions
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter", MessageId = nameof(TMovementMode), Justification = "Used as instance qualifier")]
    public interface IMovementResistanceDirectionView<TMovementMode> : IReadOnlyDynamicDataView3D<DirectionalityInformation>
    {
    }
}