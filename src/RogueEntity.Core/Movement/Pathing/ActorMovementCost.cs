using System.Collections.Generic;
using EnttSharp.Entities;
using GoRogue;
using GoRogue.Pathing;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Movement.Maps;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement.Pathing
{
    public static class ActorMovementCost
    {
        public static MoveCostDelegate ToMoveCostDelegate(this IReadOnlyMapData<MovementCost> m, DistanceCalculation distanceCalc)
        {
            bool MoveCostDelegateFn(in Coord origin, in Direction d, out float cost)
            {
                var target = origin + d;
                return m[target.X, target.Y].TryApply(distanceCalc.Calculate(d.ToCoordinates()), out cost);
            }

            return MoveCostDelegateFn;
        }

        public static IReadOnlyMapData<MovementCost> Create<TGameContext, TActorId>(TActorId actor, TGameContext context, int zLevel)
            where TGameContext : IItemContext<TGameContext, TActorId>, IMovementContext<TGameContext, TActorId>
            where TActorId : IEntityKey
        {
            var m = context.MovementCosts(actor, zLevel);

            var list = new List<IReadOnlyMapData<MovementCost>>();
            if (context.ItemResolver.TryQueryData(actor, context, out WalkingMovementData walking) && walking.Cost.CanMove(out var walkingCost))
            {
                list.Add(new WalkableCostMap(m, walkingCost));
            }
            if (context.ItemResolver.TryQueryData(actor, context, out FlyingMovementData flying) && flying.Cost.CanMove(out var flyingCost))
            {
                list.Add(new FlyingCostMap(m, flyingCost));
            }
            if (context.ItemResolver.TryQueryData(actor, context, out EtherealMovementData ether) && ether.Cost.CanMove(out var etherealCost))
            {
                list.Add(new EtherealCostMap(m, etherealCost));
            }
            if (context.ItemResolver.TryQueryData(actor, context, out SwimmingMovementData swimming) && swimming.Cost.CanMove(out var swimmingCost))
            {
                list.Add(new SwimmingCostMap(m, swimmingCost));
            }

            if (list.Count == 0)
            {
                return new ImmobileCostMap(m.Width, m.Height);
            }

            if (list.Count == 1)
            {
                return list[0];
            }

            return new CombinedCostMap(list.ToArray());
        }
    }
}