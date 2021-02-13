using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Inventory;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.Naming;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils;
using RogueEntity.Simple.BoxPusher.ItemTraits;
using static RogueEntity.Core.Movement.CostModifier.MovementCostModifiers;

namespace RogueEntity.Simple.BoxPusher
{
    public static class BoxPusherItemDefinitions
    {
        public static ReferenceItemDeclarationBuilder<TActorId> DefinePlayer<TActorId, TItemId>(this ItemDeclarationBuilderWithReferenceContext<TActorId> b)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            return b.Define("Player")
                    .AsPlayer()
                    .WithInventory()
                    .Of<TItemId>()
                    .WithGridPosition(BoxPusherMapLayers.Actors)
                    .WithLightSource(10)
                    .WithVisionSense(10)
                    .WithMovement()
                    .AsPointCost(WalkingMovement.Instance, DistanceCalculation.Euclid, 1)
                    .WithTrait(new BoxPusherPlayerProfileTrait<TActorId>())
                ;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> DefineBox<TItemId>(this ItemDeclarationBuilderWithReferenceContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define("Items.Box")
                    .WithGridPosition(BoxPusherMapLayers.Items)
                    .WithMovementCostModifier(Blocked<WalkingMovement>())
                    .WithLightResistance(0.Percent())
                    .WithTrait(new FlagItemTrait<TItemId, BoxPusherBoxMarker>(true, "BoxPusher.BoxMarkerTrait"))
                    .WithName("box")
                ;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> DefineWall<TItemId>(this ItemDeclarationBuilderWithReferenceContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define("Items.Wall")
                    .WithGridPosition(BoxPusherMapLayers.Items)
                    .AsImmobile()
                    .WithMovementCostModifier(Blocked<WalkingMovement>())
                    .WithLightResistance(100.Percent())
                    .WithName("wall");
        }

        public static BulkItemDeclarationBuilder<TItemId> DefineWall<TItemId>(this ItemDeclarationBuilderWithBulkContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define("Items.Wall")
                    .WithGridPosition(BoxPusherMapLayers.Items)
                    .AsImmobile()
                    .WithMovementCostModifier(Blocked<WalkingMovement>())
                    .WithLightResistance(100.Percent())
                    .WithName("wall");
        }

        public static BulkItemDeclarationBuilder<TItemId> DefineFloor<TItemId>(this ItemDeclarationBuilderWithBulkContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define("Items.Floor.Empty")
                    .WithGridPosition(BoxPusherMapLayers.Floor)
                    .AsImmobile()
                    .WithMovementCostModifier(For<WalkingMovement>(1))
                    .WithLightResistance(0.Percent())
                    .WithName("floor");
        }

        public static ReferenceItemDeclarationBuilder<TItemId> DefineFloorTargetZone<TItemId>(this ItemDeclarationBuilderWithReferenceContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define("Items.Floor.TargetZone")
                    .WithGridPosition(BoxPusherMapLayers.Floor)
                    .AsImmobile()
                    .WithMovementCostModifier(For<WalkingMovement>(1))
                    .WithTrait(new FlagItemTrait<TItemId, BoxPusherTargetFieldMarker>(true, "BoxPusher.TargetFieldMarkerTrait"))
                    .WithLightResistance(0.Percent())
                    .WithName("target zone");
        }
    }
}
