using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Movement.Cost
{
    public interface IMovementCostTrait<TActorId>: IItemComponentInformationTrait<TActorId, MovementCost>
        where TActorId : IEntityKey
    {
        /// <summary>
        ///   Defines how many fixed-time turn need to pass to move an actor from one grid cell to
        ///   an adjacent cell at a distance of 1.
        ///
        ///   The value returned must be greater than or equal to 1. 
        /// </summary>
        //public int TicksPerUnitOfMovement { get; }
    }
}
