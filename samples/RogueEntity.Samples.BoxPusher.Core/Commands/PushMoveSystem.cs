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

namespace RogueEntity.Samples.BoxPusher.Core.Commands
{
    public class PushMoveSystem<TActorId, TItemId> : GridMoveCommandSystem<TActorId>
        where TActorId : IEntityKey
        where TItemId : IEntityKey
    {
        public PushMoveSystem([NotNull] ITimeSource timer,
                              [NotNull] IItemResolver<TActorId> actorResolver,
                              [NotNull] IMovementDataProvider movementDataProvider,
                              [NotNull] IItemPlacementService<TActorId> placementService,
                              [NotNull] IItemResolver<TItemId> itemResolver) : base(timer, actorResolver, movementDataProvider, placementService)
        {
            
        }

        protected override bool TryPerformMove(IEntityViewControl<TActorId> v, 
                                               TActorId k, 
                                               in EntityGridPosition position, 
                                               in EntityGridPosition targetPosition, 
                                               in MovementCost movementCost)
        {
            var d = Directions.GetDirection(position.ToGridXY(), targetPosition.ToGridXY());
            if (d == Direction.None)
            {
                return false;
            }

            var boxPosition = targetPosition.WithLayer(BoxPusherMapLayers.Items);
            var boxTargetPosition = boxPosition + d.ToCoordinates();
            if (!PlacementService.TryQueryItem(boxPosition, out var box) ||
                !PlacementService.TryMoveItem(box, boxPosition, boxTargetPosition))
            {
                return false;
            }

            if (!base.TryPerformMove(v, k, in position, in targetPosition, in movementCost))
            {
                return false;
            }

            // todo
            return true;
        }
    }
}
