using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using GoRogue;
using GoRogue.Pathing;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Movement.Pathing
{
    public class EvasionPathfinderService<TGameContext, TActorId> : IEvasionPathfinder<TGameContext, TActorId>
        where TGameContext : IMovementContext<TGameContext, TActorId>, ITimeContext, IGridMapContext<TGameContext, TActorId>
        where TActorId : IEntityKey
    {
        readonly Stack<AvoidancePathfinder> cachedAvoidanceInstances;
        readonly IMovementCostViewService<TGameContext, TActorId> movementCostViewService;
        readonly MapLayerRegistry mapLayerRegistry;

        public EvasionPathfinderService(IMovementCostViewService<TGameContext, TActorId> movementCostViewService,
                                        MapLayerRegistry mapLayerRegistry,
                                        int maximumActorAwarenessDistance = 10)
        {
            if (maximumActorAwarenessDistance < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.movementCostViewService = movementCostViewService ?? throw new ArgumentNullException(nameof(movementCostViewService));
            this.mapLayerRegistry = mapLayerRegistry ?? throw new ArgumentNullException(nameof(mapLayerRegistry));
            this.cachedAvoidanceInstances = new Stack<AvoidancePathfinder>();
            this.MaximumActorAwarenessDistance = maximumActorAwarenessDistance;
        }

        public int MaximumActorAwarenessDistance { get; }

        bool TryReserveAvoidancePathfinder(out AvoidancePathfinder astar)
        {
            lock (cachedAvoidanceInstances)
            {
                if (cachedAvoidanceInstances.Count > 0)
                {
                    astar = cachedAvoidanceInstances.Pop();
                    return true;
                }

                astar = default;
                return false;
            }
        }

        void Release(AvoidancePathfinder pf)
        {
            lock (cachedAvoidanceInstances)
            {
                pf.TargetEvaluator = null;
                pf.MoveCostInformation = null;
                cachedAvoidanceInstances.Push(pf);
            }
        }

        public PathfinderResult FindClearObstructionPath(TGameContext context,
                                                         TActorId actor,
                                                         EntityGridPosition actorPosition,
                                                         out Path<EntityGridPosition> resultPath)
        {
            if (!mapLayerRegistry.TryGetValue(actorPosition.LayerId, out var mapLayer) ||
                !context.TryGetGridDataFor(mapLayer, out var actorMapData) ||
                !actorMapData.TryGetMap(actorPosition.GridZ, out var actorData, MapAccess.ReadOnly))
            {
                resultPath = default;
                return PathfinderResult.Failed;
            }

            var movementIntent = context.MovementIntent(actor, actorPosition.GridZ);
            var moveCost = movementCostViewService.CreateCostView(context, actor, actorPosition.GridZ).MoveCostView;
            if (!TryReserveAvoidancePathfinder(out var pf))
            {
                pf = new AvoidancePathfinder(context.MovementMode(actor).AsAdjacencyRule(), moveCost.Width, moveCost.Height);
            }

            var moveCostCalculation = new MovementIntentCostCalculator<TGameContext, TActorId>(context, actor, moveCost, actorData, movementIntent, MaximumActorAwarenessDistance);

            try
            {
                pf.MoveCostInformation = moveCostCalculation.MoveCost;
                pf.TargetEvaluator = moveCostCalculation.IsReservedOrOccupied;

                if (pf.FindAvoidancePath(new Coord(actorPosition.GridX, actorPosition.GridY), MaximumActorAwarenessDistance, out var path) && path.Count > 1)
                {
                    resultPath = path.ToGridPath(actorPosition);
                    return PathfinderResult.Found;
                }

                resultPath = default;
                return PathfinderResult.Failed;
            }
            finally
            {
                Release(pf);
            }
        }

        
    }
}