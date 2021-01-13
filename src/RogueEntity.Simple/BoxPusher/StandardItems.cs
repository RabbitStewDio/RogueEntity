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
        public static void DoSomething<TGameContext>(ItemDeclarationBuilder<TGameContext> t)
        {
            var itemDeclarationBuilderWithReferenceContext = t.ForEntity<ItemReference>();
            itemDeclarationBuilderWithReferenceContext.DefineWall();
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TActorId> DefinePlayer<TGameContext, TActorId, TItemId>(this ItemDeclarationBuilderWithReferenceContext<TGameContext, TActorId> b)
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

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> DefineBox<TGameContext, TItemId>(this ItemDeclarationBuilderWithReferenceContext<TGameContext, TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define("Items.Box")
                    .WithGridPosition(BoxPusherMapLayers.Items)
                    .WithLightResistance(0.Percent())
                    .WithName("box")
                ;
        }

        public static ReferenceItemDeclarationBuilder<TGameContext, TItemId> DefineWall<TGameContext, TItemId>(this ItemDeclarationBuilderWithReferenceContext<TGameContext, TItemId> b)
            where TItemId : IEntityKey
        {
            return b.Define("Items.Wall")
                    .WithGridPosition(BoxPusherMapLayers.Items)
                    .AsImmobile()
                    .WithLightResistance(100.Percent())
                    .WithName("wall");
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> DefineWall<TGameContext, TItemId>(this ItemDeclarationBuilderWithBulkContext<TGameContext, TItemId> b)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return b.Define("Items.Wall")
                    .WithGridPosition(BoxPusherMapLayers.Items)
                    .AsImmobile()
                    .WithLightResistance(100.Percent())
                    .WithName("wall");
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> DefineFloor<TGameContext, TItemId>(this ItemDeclarationBuilderWithBulkContext<TGameContext, TItemId> b)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return b.Define("Items.Floor.Empty")
                    .WithGridPosition(BoxPusherMapLayers.Floor)
                    .AsImmobile()
                    .WithLightResistance(0.Percent())
                    .WithName("floor");
        }

        public static BulkItemDeclarationBuilder<TGameContext, TItemId> DefineFloorTargetZone<TGameContext, TItemId>(this ItemDeclarationBuilderWithBulkContext<TGameContext, TItemId> b)
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