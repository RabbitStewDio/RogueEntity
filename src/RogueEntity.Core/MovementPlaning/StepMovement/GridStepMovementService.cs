using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using Serilog;

namespace RogueEntity.Core.MovementPlaning.StepMovement
{
    /// <summary>
    ///    A helper service to make it easier to work with actors and movement sources.
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

        public bool TryMoveTo(TActorId actor,
                              EntityGridPosition currentPosition,
                              EntityGridPosition targetPosition,
                              IMovementMode movementMode,
                              out double movementCost)
        {
            
            movementCost = double.MaxValue;
            
            var delta = targetPosition.ToGridXY() - currentPosition.ToGridXY();
            if (delta.X > 1 || delta.Y > 1)
            {
                // cannot move more than a single step in this service. Use a pathfinder
                // to break down larger movements into smaller unit-steps.
                return false;
            }
            
            var direction = Directions.GetDirection(delta);
            if (direction == Direction.None)
            {
                // Current position and target position must be the same.
                return false;
            }
                
            if (itemResolver.TryQueryData(actor, out AggregateMovementCostFactors mcf) ||
                !mcf.TryGetMovementCost(movementMode, out var cost))
            {
                // have no movement modes. That means that actor cannot move on its own.
                return false;
            }
            
            
            var currentRawMovementCost = cost.MovementStyle.Calculate(currentPosition, targetPosition);
            if (currentRawMovementCost > cost.MovementStyle.MaximumStepDistance())
            {
                // cannot move more than a single step in this service. Use a pathfinder
                // to break down larger movements into smaller unit-steps.
                //
                // This condition branch filters out Manhattan-Distance violations.
                return false;
            }

            if (!dataProvider.MovementCosts.TryGetValue(cost.MovementMode, out var movementSourceData))
            {
                // No resistance data or movement data for this movement mode. This usually 
                // points to a configuration error.
                Logger.Warning("Unable to locate movement data for movement mode {MovementMode}", cost.MovementMode);
                return false;
            }

            if (!movementSourceData.OutboundDirections.TryGetView(currentPosition.GridZ, out var view))
            {
                // The movement data should have been computed for all active layers where actors exist.
                // If that has not happened, you might have moved this actor before the data can be
                // refreshed, which would mean your system ordering is wrong to allow that to happen.
                Logger.Warning("Unable to locate outbound direction movement data for actor position {Position}", currentPosition);
                return false;
            }
                
            var data = view[currentPosition.GridX, currentPosition.GridY];
            if (!data.IsMovementAllowed(direction))
            {
                return false;
            }

            movementCost = cost.Cost * currentRawMovementCost;
            return true;

        }
        public bool TryMoveTo(TActorId actor, EntityGridPosition currentPosition, EntityGridPosition targetPosition, 
                              out IMovementMode movementMode, out double movementCost)
        {
            movementCost = double.MaxValue;
            movementMode = null;
            
            var delta = targetPosition.ToGridXY() - currentPosition.ToGridXY();
            if (delta.X > 1 || delta.Y > 1)
            {
                // cannot move more than a single step in this service. Use a pathfinder
                // to break down larger movements into smaller unit-steps.
                return false;
            }
            
            var direction = Directions.GetDirection(delta);
            if (direction == Direction.None)
            {
                // Current position and target position must be the same.
                return false;
            }
                
            if (itemResolver.TryQueryData(actor, out AggregateMovementCostFactors mcf))
            {
                // have no movement modes. That means that actor cannot move on its own.
                return false;
            }

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
                if (movementMode == null || actualMovementCost < movementCost)
                {
                    movementMode = cost.MovementMode;
                    movementCost = cost.Cost * currentRawMovementCost;
                }
            }

            return movementMode != null;
        }
    }
}
