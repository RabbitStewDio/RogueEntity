using System;
using System.Collections.Generic;
using EnttSharp.Entities;
using GoRogue;
using GoRogue.Pathing;
using RogueEntity.Core.Infrastructure.Positioning.Grid;

namespace RogueEntity.Core.Movement.Pathing
{
    public class PathfinderService<TGameContext, TActorId> : IPathfinderService<TGameContext, TActorId>
        where TGameContext : IMovementContext<TGameContext, TActorId> 
        where TActorId : IEntityKey
    {
        readonly Stack<SingleGoalAStar> cachedBasicInstances;
        readonly IMovementCostViewService<TGameContext, TActorId> movementCostViewService;

        public PathfinderService(IMovementCostViewService<TGameContext, TActorId> movementCostViewService)
        {
            this.movementCostViewService = movementCostViewService ?? throw new ArgumentNullException(nameof(movementCostViewService));
            cachedBasicInstances = new Stack<SingleGoalAStar>();
        }

        bool TryReserveBasicPathfinder(out SingleGoalAStar astar)
        {
            lock (cachedBasicInstances)
            {
                if (cachedBasicInstances.Count > 0)
                {
                    astar = cachedBasicInstances.Pop();
                    return true;
                }

                astar = default;
                return false;
            }
        }

        void Release(SingleGoalAStar astar)
        {
            lock (cachedBasicInstances)
            {
                cachedBasicInstances.Push(astar);
            }
        }


        public PathfinderResult FindPath(TGameContext context,
                                         TActorId actor,
                                         EntityGridPosition startPosition,
                                         EntityGridPosition targetPos,
                                         float targetDistance,
                                         out Path<EntityGridPosition> resultPath)
        {
            var distanceCalculation = context.MovementMode(actor);
            if (distanceCalculation.Calculate(startPosition, targetPos) <= targetDistance)
            {
                resultPath = default;
                return PathfinderResult.Arrived;
            }

            var moveCost = movementCostViewService.CreateCostView(context, actor, startPosition.GridZ).MoveCostView;
            var moveCostDelegate = moveCost.ToMoveCostDelegate(distanceCalculation);

            if (TryReserveBasicPathfinder(out var pf))
            {
                pf.Reset(moveCostDelegate, moveCost.Width, moveCost.Height);
            }
            else
            {
                pf = new SingleGoalAStar(moveCostDelegate, distanceCalculation, moveCost.Width, moveCost.Height);
            }

            try
            {
                pf.TargetLocation = new Coord(targetPos.GridX, targetPos.GridY);
                pf.TargetDistance = targetDistance;
                if (!pf.TryFindPath(new Coord(startPosition.GridX, startPosition.GridY), out var discoveredPath))
                {
                    resultPath = default;
                    return PathfinderResult.Failed;
                }

                resultPath = discoveredPath.ToGridPath(startPosition);
                return PathfinderResult.Found;
            }
            finally
            {
                Release(pf);
            }
        }
    }
}