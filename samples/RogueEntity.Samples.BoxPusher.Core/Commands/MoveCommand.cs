using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.MovementPlaning.StepMovement;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using System;

namespace RogueEntity.Samples.BoxPusher.Core.Commands
{
    public readonly struct MoveCommand
    {
        public readonly EntityGridPosition MoveTo;
        public readonly Optional<int> FinishTurn;
        public readonly Optional<IMovementMode> movementMode;

        public MoveCommand(EntityGridPosition moveTo, Optional<int> finishTurn = default)
        {
            MoveTo = moveTo;
            FinishTurn = finishTurn;
        }
    }

    public class MoveCommandSystem<TActorId>
        where TActorId : IEntityKey
    {
        readonly ITimeSource timer;
        readonly IItemResolver<TActorId> itemResolver;
        readonly IMovementDataProvider movementDataProvider;
        readonly IItemPlacementService<TActorId> placementService;
        readonly GridStepMovementService<TActorId> movementService;


        public MoveCommandSystem([NotNull] ITimeSource timer,
                                 [NotNull] IItemResolver<TActorId> itemResolver,
                                 [NotNull] IMovementDataProvider movementDataProvider,
                                 [NotNull] IItemPlacementService<TActorId> placementService)
        {
            this.timer = timer ?? throw new ArgumentNullException(nameof(timer));
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.movementDataProvider = movementDataProvider ?? throw new ArgumentNullException(nameof(movementDataProvider));
            this.placementService = placementService ?? throw new ArgumentNullException(nameof(placementService));
            movementService = new GridStepMovementService<TActorId>(itemResolver, movementDataProvider);
        }

        public void ProcessMovement(IEntityViewControl<TActorId> v,
                                    TActorId k,
                                    in PlayerObserverTag observer,
                                    in EntityGridPosition position,
                                    ref CommandInProgress progressIndicator,
                                    ref MoveCommand cmd)
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

            if (cmd.FinishTurn.TryGetValue(out var turn))
            {
                if (timer.FixedStepTime >= turn)
                {
                    progressIndicator = progressIndicator.MarkHandled();
                }

                return;
            }

            double movementCost;
            if (cmd.movementMode.TryGetValue(out var movementMode))
            {
                if (!movementService.TryMoveTo(k, position, moveTarget, movementMode, out movementCost))
                {
                    return;
                }
                
            }
            else if (!movementService.TryMoveTo(k, position, moveTarget, out movementMode, out movementCost))
            {
                return;
            }
            
            // 
            
        }
    }
}
