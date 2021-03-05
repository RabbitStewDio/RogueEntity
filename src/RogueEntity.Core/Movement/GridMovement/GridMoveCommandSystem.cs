using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Time;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using System;

namespace RogueEntity.Core.Movement.GridMovement
{
    public class GridMoveCommandSystem<TActorId>
        where TActorId : IEntityKey
    {
        protected readonly ITimeSource Timer;
        protected readonly IItemPlacementService<TActorId> PlacementService;
        protected readonly GridStepMovementService<TActorId> MovementService;


        public GridMoveCommandSystem([NotNull] ITimeSource timer,
                                     [NotNull] IItemResolver<TActorId> actorResolver,
                                     [NotNull] IMovementDataProvider movementDataProvider,
                                     [NotNull] IItemPlacementService<TActorId> placementService)
        {
            this.Timer = timer ?? throw new ArgumentNullException(nameof(timer));
            this.PlacementService = placementService ?? throw new ArgumentNullException(nameof(placementService));
            this.MovementService = new GridStepMovementService<TActorId>(actorResolver, movementDataProvider);
        }

        public void ProcessMovement(IEntityViewControl<TActorId> v,
                                    TActorId k,
                                    in PlayerObserverTag observer,
                                    in EntityGridPosition position,
                                    ref CommandInProgress progressIndicator,
                                    ref GridMoveCommand cmd)
        {
            if (progressIndicator.Handled)
            {
                return;
            }

            if (position.IsInvalid)
            {
                progressIndicator = progressIndicator.MarkHandled();
                return;
            }

            var moveTarget = cmd.MoveTo;
            if (moveTarget.IsInvalid)
            {
                progressIndicator = progressIndicator.MarkHandled();
                return;
            }

            var movementDistance = DistanceCalculation.Manhattan.Calculate(moveTarget, position);
            if (movementDistance == 0 || movementDistance > 1)
            {
                progressIndicator = progressIndicator.MarkHandled();
                return;
            }

            if (cmd.FinishTime.TryGetValue(out var turn))
            {
                if (Timer.CurrentTime >= turn)
                {
                    progressIndicator = progressIndicator.MarkHandled();
                }

                return;
            }

            MovementCost movementCost;
            if (cmd.MovementMode.TryGetValue(out var movementMode))
            {
                if (!MovementService.CanMoveTo(k, position, moveTarget, movementMode, out movementCost))
                {
                    progressIndicator = progressIndicator.MarkHandled();
                    return;
                }
            }
            else if (!MovementService.CanMoveTo(k, position, moveTarget, out movementCost))
            {
                progressIndicator = progressIndicator.MarkHandled();
                return;
            }

            if (!TryPerformMove(v, k, position, moveTarget, movementCost))
            {
                progressIndicator = progressIndicator.MarkHandled();
                return;
            }

            // we have: movement mode, total movement cost, we need the time the movement will be finished.
            var distance = movementCost.MovementStyle.Calculate(position, moveTarget);
            var velocity = movementCost.ToMeterPerSecond(Timer.TimeSourceDefinition);
            cmd = cmd.WithFinishTime(movementCost.MovementMode, Timer.CurrentTime.Add(TimeSpan.FromSeconds(distance / velocity)));
        }

        protected virtual bool TryPerformMove(IEntityViewControl<TActorId> v,
                                              TActorId k,
                                              in EntityGridPosition position,
                                              in EntityGridPosition targetPosition,
                                              in MovementCost movementCost)
        {
            if (!PlacementService.TryMoveItem(k, position, targetPosition))
            {
                // unable to actually move the actor to its new position.
                return false;
            }

            return true;
        }

        public void CleanUpMoveAction(IEntityViewControl<TActorId> v,
                                      TActorId k,
                                      in CommandInProgress progressIndicator,
                                      in GridMoveCommand cmd)
        {
            if (!progressIndicator.Handled)
            {
                return;
            }

            var id = CommandTypeId.Create<GridMoveCommand>();
            if (progressIndicator.ActiveCommand.TryGetValue(out var value) && value == id)
            {
                v.RemoveComponent<CommandInProgress>(k);
                v.RemoveComponent<GridMoveCommand>(k);
            }
        }
    }
}
