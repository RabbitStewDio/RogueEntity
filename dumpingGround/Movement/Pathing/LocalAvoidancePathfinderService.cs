using System.Collections.Generic;
using EnTTSharp.Entities;
using GoRogue;
using GoRogue.Pathing;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Movement.Pathing
{
    public class LocalAvoidancePathfinderService<TGameContext, TActorId>
        where TActorId : IEntityKey
        where TGameContext : IMovementContext<TGameContext, TActorId>, ITimeContext, IGridMapContext<TGameContext, TActorId>
    {
        static readonly ILogger logger = SLog.ForContext(typeof(LocalAvoidancePathfinderService<TGameContext, TActorId>));

        public const int MaximumActorAwarenessDistance = 4;

        readonly Stack<MultiGoalAStar> cachedInstances;
        readonly IMovementCostViewService<TGameContext, TActorId> movementCostViewService;
        readonly MapLayerRegistry mapLayerRegistry;

        public LocalAvoidancePathfinderService(IMovementCostViewService<TGameContext, TActorId> movementCostViewService, 
                                          MapLayerRegistry mapLayerRegistry)
        {
            this.movementCostViewService = movementCostViewService;
            this.mapLayerRegistry = mapLayerRegistry;
            cachedInstances = new Stack<MultiGoalAStar>(16);
        }

        bool TryReserveMultiGoalPathfinder(out MultiGoalAStar astar)
        {
            lock (cachedInstances)
            {
                if (cachedInstances.Count > 0)
                {
                    astar = cachedInstances.Pop();
                    return true;
                }

                astar = default;
                return false;
            }
        }

        void Release(MultiGoalAStar astar)
        {
            lock (cachedInstances)
            {
                cachedInstances.Push(astar);
            }
        }

        public PathfinderResult FindObstacleAvoidancePath(TGameContext context,
                                                          TActorId actor,
                                                          EntityGridPosition currentPosition,
                                                          Path<EntityGridPosition> currentPath,
                                                          float targetDistance,
                                                          out Path<EntityGridPosition> resultPath)
        {
            if (!currentPath.TryGetTail(out var end) ||
                !mapLayerRegistry.TryGetValue(currentPosition.LayerId, out var mapLayer) ||
                !context.TryGetGridDataFor(mapLayer, out var actorMapData) ||
                !actorMapData.TryGetMap(currentPosition.GridZ, out var actorData, MapAccess.ReadOnly))
            {
                logger.Warning("Avoidance path prerequisites not met");
                resultPath = default;
                return PathfinderResult.Failed;
            }

            var movementIntent = context.MovementIntent(actor, currentPosition.GridZ);
            var moveCost = movementCostViewService.CreateCostView(context, actor, currentPosition.GridZ).MoveCostView;
            var moveCostCalculation = new MovementIntentCostCalculator<TGameContext, TActorId>(context, actor, moveCost, actorData, movementIntent, MaximumActorAwarenessDistance);

            if (TryReserveMultiGoalPathfinder(out var pf))
            {
                pf.Reset(moveCostCalculation.MoveCost, moveCost.Width, moveCost.Height);
            }
            else
            {
                pf = new MultiGoalAStar(context.MovementMode(actor), 
                                        moveCostCalculation.MoveCost, 
                                        moveCost.Width, moveCost.Height);
            }

            pf.AddTarget(new Coord(end.GridX, end.GridY));
            AddExistingPathAsGoals(pf, currentPosition, currentPath);

            var currentPosAsCoord = new Coord(currentPosition.GridX, currentPosition.GridY);
            if (!pf.TryFindPath(currentPosAsCoord, out var discoveredPath, out _) ||
                discoveredPath.Count <= 1)
            {
                logger.Verbose("Unable to find avoidance path from {StartPosition} to {EndPosition}", currentPosition, end);
                Release(pf);
                resultPath = default;
                return PathfinderResult.Failed;
            }

            Release(pf);
            resultPath = MergeResultPath(currentPosition, currentPath, discoveredPath);
            logger.Verbose("Found avoidance path from {StartPosition} to {EndPosition} as {Path} resulting in {SplicedPath}",
                           currentPosition, end, discoveredPath, resultPath);
            return PathfinderResult.Found;
        }

        static void AddExistingPathAsGoals(MultiGoalAStar pf, 
                                           EntityGridPosition currentPosition, 
                                           Path<EntityGridPosition> currentPath)
        {
            pf.ClearTargets();
            var foundPath = false;
            foreach (var t in currentPath)
            {
                if (t == currentPosition)
                {
                    foundPath = true;
                    continue;
                }

                if (foundPath)
                {
                    pf.AddTarget(new Coord(t.GridX, t.GridY));
                }
            }
        }

        static Path<EntityGridPosition> MergeResultPath(EntityGridPosition currentPosition, 
                                                        Path<EntityGridPosition> currentPath, 
                                                        Path<Coord> discoveredPath)
        {
            bool foundPath;
            var path = new List<EntityGridPosition>();
            foreach (var t in discoveredPath)
            {
                path.Add(EntityGridPosition.OfRaw(currentPosition.LayerId, t.X, t.Y, currentPosition.GridZ));
            }

            if (path.Count == 0)
            {
                return currentPath;
            }

            var discoveredPathEnd = path[path.Count - 1];
            foundPath = false;
            // merge both paths back together.
            foreach (var t in currentPath)
            {
                if (t == discoveredPathEnd)
                {
                    foundPath = true;
                    continue;
                }

                if (foundPath)
                {
                    path.Add(t);
                }
            }

            return new Path<EntityGridPosition>(path.ToArray(), 0);
        }
    }
}