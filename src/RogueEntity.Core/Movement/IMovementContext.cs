using GoRogue;
using RogueEntity.Core.Infrastructure.Positioning.Grid;
using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Movement.Maps;
using RogueEntity.Core.Movement.Pathing;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement
{
    public interface IMovementContext<TGameContext, TActorId> 
        where TGameContext : IMovementContext<TGameContext, TActorId>
    {
        DistanceCalculation MovementMode(TActorId actor);
        IReadOnlyMapData<MovementCostProperties> MovementCosts(TActorId actor, int zLevel);
        IReadOnlyMapData<MovementAllowedProperties> MovementAllowed(TActorId actor, int zLevel);
        IReadOnlyMapData<MovementIntent<TActorId>> MovementIntent(TActorId actor, int zLevel);
    }

    public static class IMovementContextExtensions
    {
        public static float Calculate(this DistanceCalculation calc,
                                      EntityGridPosition posA,
                                      EntityGridPosition posB)
        {
            return calc.Calculate(posA.GridX, posA.GridY, posA.GridZ, posB.GridX, posB.GridY, posB.GridZ);
        }
    }
}