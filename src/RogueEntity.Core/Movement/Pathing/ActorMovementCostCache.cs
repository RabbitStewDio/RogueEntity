using EnTTSharp.Entities.Attributes;
using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement.Pathing
{
    /// <summary>
    ///   A transient cache of pathfinding data.
    /// </summary>
    [EntityComponent]
    public readonly struct ActorMovementCostCache
    {
        public readonly IReadOnlyMapData<MovementCost> MoveCostView;
        public readonly int ZLevel;

        public ActorMovementCostCache(IReadOnlyMapData<MovementCost> moveCostView, int zLevel)
        {
            this.MoveCostView = moveCostView;
            this.ZLevel = zLevel;
        }

        public bool IsValid(int zLevel) => MoveCostView != null && zLevel == ZLevel;
    }
}