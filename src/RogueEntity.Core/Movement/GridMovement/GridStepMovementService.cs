using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using Serilog;

namespace RogueEntity.Core.Movement.GridMovement
{
    /// <summary>
    ///    A helper service to make it easier to work with actors and movement sources.
    ///    The service computes the standard movement cost for actors. Actual movement
    ///    (aka modifying the map) is done via the IItemPlacementService.
    /// </summary>
    public class GridStepMovementService<TActorId>
        where TActorId : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<GridStepMovementService<TActorId>>();
        readonly IItemResolver<TActorId> itemResolver;
        readonly IMovementDataProvider dataProvider;

        public GridStepMovementService(IItemResolver<TActorId> itemResolver,
                                       IMovementDataProvider dataProvider)
        {
            this.itemResolver = itemResolver;
            this.dataProvider = dataProvider;
        }

        public bool CanMoveTo(TActorId actor,
                              EntityGridPosition currentPosition,
                              EntityGridPosition targetPosition,
                              IMovementMode movementMode,
                              out MovementCost movementCost)
        {
            var delta = targetPosition.ToGridXY() - currentPosition.ToGridXY();
            if (delta.X > 1 || delta.Y > 1)
            {
                // cannot move more than a single step in this service. Use a pathfinder
                // to break down larger movements into smaller unit-steps.
                movementCost = default;
                return false;
            }

            var direction = Directions.GetDirection(delta);
            if (direction == Direction.None)
            {
                // Current position and target position must be the same.
                movementCost = default;
                return false;
            }

            if (!itemResolver.TryQueryData(actor, out AggregateMovementCostFactors mcf) ||
                !mcf.TryGetMovementCost(movementMode, out var cost))
            {
                // have no movement modes. That means that actor cannot move on its own.
                movementCost = default;
                return false;
            }


            var currentRawMovementCost = cost.MovementStyle.Calculate(currentPosition, targetPosition);
            if (currentRawMovementCost > cost.MovementStyle.MaximumStepDistance())
            {
                // cannot move more than a single step in this service. Use a pathfinder
                // to break down larger movements into smaller unit-steps.
                //
                // This condition branch filters out Manhattan-Distance violations.
                movementCost = default;
                return false;
            }

            if (!dataProvider.MovementCosts.TryGetValue(cost.MovementMode, out var movementSourceData))
            {
                // No resistance data or movement data for this movement mode. This usually 
                // points to a configuration error.
                Logger.Warning("Unable to locate movement data for movement mode {MovementMode}", cost.MovementMode);
                movementCost = default;
                return false;
            }

            if (!movementSourceData.OutboundDirections.TryGetView(currentPosition.GridZ, out var view))
            {
                // The movement data should have been computed for all active layers where actors exist.
                // If that has not happened, you might have moved this actor before the data can be
                // refreshed, which would mean your system ordering is wrong to allow that to happen.
                Logger.Warning("Unable to locate outbound direction movement data for actor position {Position}", currentPosition);
                movementCost = default;
                return false;
            }

            var data = view[currentPosition.GridX, currentPosition.GridY];
            if (!data.IsMovementAllowed(direction))
            {
                movementCost = default;
                return false;
            }

            movementCost = cost;
            return true;
        }

        public bool CanMoveTo(TActorId actor,
                              EntityGridPosition currentPosition,
                              EntityGridPosition targetPosition,
                              out MovementCost movementCost)
        {

            var delta = targetPosition.ToGridXY() - currentPosition.ToGridXY();
            if (delta.X > 1 || delta.Y > 1)
            {
                // cannot move more than a single step in this service. Use a pathfinder
                // to break down larger movements into smaller unit-steps.
                movementCost = default;
                return false;
            }

            var direction = Directions.GetDirection(delta);
            if (direction == Direction.None)
            {
                // Current position and target position must be the same.
                movementCost = default;
                return false;
            }

            if (!itemResolver.TryQueryData(actor, out AggregateMovementCostFactors mcf))
            {
                // have no movement modes. That means that actor cannot move on its own.
                movementCost = default;
                return false;
            }

            var m = Optional.Empty<(MovementCost m, double totalCost)>();
            foreach (var cost in mcf.MovementCosts)
            {
                var currentRawMovementCost = cost.MovementStyle.Calculate(currentPosition, targetPosition);
                if (currentRawMovementCost > cost.MovementStyle.MaximumStepDistance())
                {
                    // cannot move more than a single step in this service. Use a pathfinder
                    // to break down larger movements into smaller unit-steps.
                    //
                    // This condition branch filters out Manhattan-Distance violations.
                    continue;
                }

                if (!dataProvider.MovementCosts.TryGetValue(cost.MovementMode, out var movementSourceData))
                {
                    // No resistance data or movement data for this movement mode. This usually 
                    // points to a configuration error.
                    Logger.Warning("Unable to locate movement data for movement mode {MovementMode}", cost.MovementMode);
                    continue;
                }

                if (!movementSourceData.OutboundDirections.TryGetView(currentPosition.GridZ, out var view))
                {
                    // The movement data should have been computed for all active layers where actors exist.
                    // If that has not happened, you might have moved this actor before the data can be
                    // refreshed, which would mean your system ordering is wrong to allow that to happen.
                    Logger.Warning("Unable to locate outbound direction movement data for actor position {Position}", currentPosition);
                    continue;
                }

                var data = view[currentPosition.GridX, currentPosition.GridY];
                if (!data.IsMovementAllowed(direction))
                {
                    continue;
                }

                var actualMovementCost = cost.Cost * currentRawMovementCost;
                if (!m.TryGetValue(out var mv) || actualMovementCost < mv.totalCost)
                {
                    m = (cost, actualMovementCost);
                }
            }

            if (m.TryGetValue(out var xx))
            {
                movementCost = xx.m;
                return true;
            }

            movementCost = default;
            return false;
        }
    }
}
