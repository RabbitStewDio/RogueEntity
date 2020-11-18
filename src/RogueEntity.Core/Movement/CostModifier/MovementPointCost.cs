using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Movement.CostModifier
{
    /// <summary>
    ///   Encodes movement costs as an absolute cost per tile moved. Games like Heroes of Magic provide each unit with a budget of movement points,
    ///   and each terrain then has an associated movement point cost that is consumed for each tile the character moves.
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminator")]
    public readonly struct MovementPointCost<TMovementMode>
    {
        public readonly float Cost;

        public MovementPointCost(float cost)
        {
            Cost = cost;
        }
    }
}