﻿using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Movement.Pathing
{
    public class MovementCostViewService<TGameContext, TActorId> : IMovementCostViewService<TGameContext, TActorId>
        where TActorId : IEntityKey
        where TGameContext : IItemContext<TGameContext, TActorId>, IMovementContext<TGameContext, TActorId>
    {
        public ActorMovementCostCache CreateCostView(TGameContext context, TActorId actor, int zLevel)
        {
            if (context.ItemResolver.TryQueryData(actor, context, out ActorMovementCostCache cached))
            {
                if (cached.IsValid(zLevel))
                {
                    return cached;
                }
            }

            var costMap = ActorMovementCost.Create(actor, context, zLevel);

            if (context.ItemResolver.TryQueryData(actor, context, out DiscoveryMapData discoveryMap))
            {
                costMap = new RestrictedCostMap(costMap, discoveryMap.Map);
            }

            cached = new ActorMovementCostCache(costMap, zLevel);
            context.ItemResolver.TryUpdateData(actor, context, in cached, out _);
            return cached;
        }

        public class RestrictedCostMap : IReadOnlyMapData<MovementCost>
        {
            readonly IReadOnlyMapData<MovementCost> costData;
            readonly IReadOnlyMapData<bool> discoveryMap;

            public RestrictedCostMap(IReadOnlyMapData<MovementCost> costData, 
                                     IReadOnlyMapData<bool> discoveryMap)
            {
                this.costData = costData;
                this.discoveryMap = discoveryMap;
            }

            public int Width => costData.Width;

            public int Height => costData.Height;

            public MovementCost this[int x, int y]
            {
                get
                {
                    if (discoveryMap[x, y])
                    {
                        return costData[x, y];
                    }
                    return MovementCost.Blocked;
                }
            }
        }
    }
}