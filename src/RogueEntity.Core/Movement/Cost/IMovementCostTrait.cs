using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Movement.Cost
{
    public interface IMovementCostTrait<TActorId>: IItemComponentInformationTrait<TActorId, MovementCost>
        where TActorId : struct, IEntityKey
    {
    }
}
