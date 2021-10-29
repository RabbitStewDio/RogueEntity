using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Time;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.GridMovement;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using System;

namespace RogueEntity.Samples.BoxPusher.Core.Commands
{
    public class PushMoveCommandSystem<TActorId, TItemId> : GridMoveCommandSystem<TActorId>
        where TActorId : IEntityKey
        where TItemId : IEntityKey
    {
        readonly IItemResolver<TItemId> itemResolver;
        readonly IItemPlacementService<TItemId> itemPlacementService;

        public PushMoveCommandSystem([NotNull] Lazy<ITimeSource> timer,
                              [NotNull] IItemResolver<TActorId> actorResolver,
                              [NotNull] IMovementDataProvider movementDataProvider,
                              [NotNull] IItemPlacementService<TActorId> actorPlacementService,
                              [NotNull] IItemResolver<TItemId> itemResolver,
                              [NotNull] IItemPlacementService<TItemId> itemPlacementService) : base(timer, actorResolver, movementDataProvider, actorPlacementService)
        {
            this.itemResolver = itemResolver;
            this.itemPlacementService = itemPlacementService;
        }

        protected override bool TryPerformMove(IEntityViewControl<TActorId> v,
                                               TActorId k,
                                               in EntityGridPosition position,
                                               in EntityGridPosition targetPosition,
                                               in MovementCost movementCost,
                                               in TimeSpan expectedMovementDuration)
        {
            var d = Directions.GetDirection(position.ToGridXY(), targetPosition.ToGridXY());
            if (d == Direction.None)
            {
                return false;
            }

            var boxPosition = targetPosition.WithLayer(BoxPusherMapLayers.Items);
            var boxTargetPosition = boxPosition + d.ToCoordinates();
            if (!itemPlacementService.TryQueryItem(boxPosition, out var box))
            {
                return false;
            }

            if (box.IsEmpty)
            {
                // no box, so normal movement can take place.
                return base.TryPerformMove(v, k, in position, in targetPosition, in movementCost, expectedMovementDuration);
            }
            
            if (!itemResolver.IsItemType(box, BoxPusherItemDefinitions.Box.Id))
            {
                // must be a wall or some other item that we dont understand or handle here.
                // in this method, we expect either an empty space or a box. Abort movement.
                return false;
            }
            
            if (!itemPlacementService.TryMoveItem(box, boxPosition, boxTargetPosition))
            {
                return false;
            }

            if (!base.TryPerformMove(v, k, in position, in targetPosition, in movementCost, expectedMovementDuration))
            {
                return false;
            }

            

            // todo
            return true;
        }
    }
}
