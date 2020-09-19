using System.Collections.Generic;
using GoRogue;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement.Pathing
{
    public readonly struct MovementIntentCostCalculator<TGameContext, TActorId> 
        where TGameContext : ITimeContext
    {
        static readonly IEqualityComparer<TActorId> EqualityComparer = EqualityComparer<TActorId>.Default;
        readonly TGameContext context;
        readonly TActorId actor;
        readonly IReadOnlyMapData<MovementCost> baseMoveCost;
        readonly IReadOnlyMapData<TActorId> actorPositions;
        readonly IReadOnlyMapData<MovementIntent<TActorId>> reservationMap;
        readonly int maximumActorAwarenessDistance;

        public MovementIntentCostCalculator(TGameContext context,
                                                   TActorId actor,
                                                   IReadOnlyMapData<MovementCost> baseMoveCost,
                                                   IReadOnlyMapData<TActorId> actorPositions,
                                                   IReadOnlyMapData<MovementIntent<TActorId>> reservationMap,
                                                   int maximumActorAwarenessDistance)
        {
            this.context = context;
            this.actor = actor;
            this.baseMoveCost = baseMoveCost;
            this.actorPositions = actorPositions;
            this.reservationMap = reservationMap;
            this.maximumActorAwarenessDistance = maximumActorAwarenessDistance;
        }

        public bool MoveCost(in Coord origin, in Direction d, out float cost)
        {
            var target = origin + d;
            if (!EqualityComparer.Equals(actorPositions[target.X, target.Y], default))
            {
                // dont move into cells that are currently occupied by other actors.
                cost = default;
                return false;
            }

            // Calculate a extra movement penalty for attempting to move through an already occupied cell.
            // This should make the actor avoid cells that will be used by other actors.
            // Turns cost is a value between [100% to 200%] depending on how urgently a cell is reserved.
            //
            // Note: There is probably good potential for optimization here, as we dont check whether the
            // reserved path actually conflicts with the plotted path.
            var reservedCost = reservationMap[target.X, target.Y];
            reservedCost.IsValidMoveTarget(context, actor, out var reservationTime);
            var turnsCost = 1 + (reservationTime).Clamp(0, maximumActorAwarenessDistance) / (float)maximumActorAwarenessDistance;

            var mc = baseMoveCost[target.X, target.Y];
            return mc.TryApply(1 + turnsCost, out cost);
        }

        public bool IsReservedOrOccupied(Coord target)
        {
            if (!EqualityComparer.Equals(actorPositions[target.X, target.Y], default))
            {
                // dont move into cells that are currently occupied by other actors.
                return true;
            }

            var reservedCost = reservationMap[target.X, target.Y];
            return reservedCost.IsExpired(context);
        }
    }
}