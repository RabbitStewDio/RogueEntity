using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Inventory;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Meta.Naming;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils;

namespace RogueEntity.Simple.BoxPusher
{
    public static class StandardItems
    {
        public static void DoSomething(ItemDeclarationBuilder t)
        {
            var itemDeclarationBuilderWithReferenceContext = t.ForEntity<ItemReference>();
            itemDeclarationBuilderWithReferenceContext.DefineWall();
        }

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
                ;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> DefineBox<TItemId>(this ItemDeclarationBuilderWithReferenceContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define("Items.Box")
                    .WithGridPosition(BoxPusherMapLayers.Items)
                    .WithLightResistance(0.Percent())
                    .WithName("box")
                ;
        }

        public static ReferenceItemDeclarationBuilder<TItemId> DefineWall<TItemId>(this ItemDeclarationBuilderWithReferenceContext<TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define("Items.Wall")
                    .WithGridPosition(BoxPusherMapLayers.Items)
                    .AsImmobile()
                    .WithLightResistance(100.Percent())
                    .WithName("wall");
        }

        public static BulkItemDeclarationBuilder<TItemId> DefineWall<TItemId>(this ItemDeclarationBuilderWithBulkContext<TItemId> b)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return b.Define("Items.Wall")
                    .WithGridPosition(BoxPusherMapLayers.Items)
                    .AsImmobile()
                    .WithLightResistance(100.Percent())
                    .WithName("wall");
        }

        public static BulkItemDeclarationBuilder<TItemId> DefineFloor<TItemId>(this ItemDeclarationBuilderWithBulkContext<TItemId> b)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return b.Define("Items.Floor.Empty")
                    .WithGridPosition(BoxPusherMapLayers.Floor)
                    .AsImmobile()
                    .WithLightResistance(0.Percent())
                    .WithName("floor");
        }

        public static BulkItemDeclarationBuilder<TItemId> DefineFloorTargetZone<TItemId>(this ItemDeclarationBuilderWithBulkContext<TItemId> b)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return b.Define("Items.Floor.TargetZone")
                    .WithGridPosition(BoxPusherMapLayers.Floor)
                    .AsImmobile()
                    .WithLightResistance(0.Percent())
                    .WithName("target zone");
        }
    }
}