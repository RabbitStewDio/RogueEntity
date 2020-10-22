using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Movement.ItemCosts
{
    public interface IMovementTrait<TGameContext, TActorId> : IReferenceItemTrait<TGameContext, TActorId> 
        where TActorId : IEntityKey
    {
        MovementCost BaseMovementCost { get; }

        bool CalculateVariableCellCost(TGameContext context,
                                       Position position,
                                       out MovementCost movementCost);

        bool CanEnterCell(TActorId k, 
                          TGameContext context,
                          Position position,
                          out MovementCost movementCost);
    }
}