using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Generator.Tests.Fixtures;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Generator.Tests
{
    [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
    public abstract class MapOperationsFixtureBase : WhenFixtureSupport 
    {
        protected static readonly MapLayer FloorLayer = new MapLayer(1, "Floor");
        protected static readonly MapLayer ItemLayer = new MapLayer(2, "Items");
        protected static readonly MapLayer ActorLayer = new MapLayer(3, "Actors");

        protected static readonly ItemDeclarationId BulkFloor1 = "Bulk.Floor.1";
        protected static readonly ItemDeclarationId BulkFloor2 = "Bulk.Floor.2";
        protected static readonly ItemDeclarationId BulkFloor3 = "Bulk.Floor.3";
        protected static readonly ItemDeclarationId BulkItem1 = "Bulk.Item.1";
        protected static readonly ItemDeclarationId BulkItem2 = "Bulk.Item.2";
        protected static readonly ItemDeclarationId BulkItem3 = "Bulk.Item.3";
        protected static readonly ItemDeclarationId ReferenceItem1 = "Reference.Item.1";
        protected static readonly ItemDeclarationId Actor = "Actor";

        protected ItemContextBackend<ItemReference> ItemEntityContext;
        protected IMapContext<ItemReference> ItemMapContext;
        protected ItemPlacementServiceContext<ItemReference> ItemPlacementContext;
        protected ItemContextBackend<ActorReference> ActorEntityContext;
        protected IMapContext<ActorReference> ActorMapContext;
        protected ItemPlacementServiceContext<ActorReference> ActorPlacementContext;
        protected MapBuilder MapBuilder;
        protected ItemFixture<ItemReference> Items;
        protected ItemFixture<ActorReference> Actors;

        [SetUp]
        public void SetUp()
        {
            ItemMapContext = new DefaultMapContext<ItemReference>(DynamicDataViewConfiguration.Default16X16)
                             .WithBasicGridMapLayer(FloorLayer)
                             .WithBasicGridMapLayer(ItemLayer);
            ItemEntityContext = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            ItemEntityContext.EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            ItemEntityContext.EntityRegistry.Register<DestroyedMarker>();
            ItemEntityContext.EntityRegistry.Register<CascadingDestroyedMarker>();
            ItemEntityContext.EntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkFloor1)
                                                    .WithTrait(new BulkItemGridPositionTrait<ItemReference>(BodySize.OneByOne, FloorLayer))
                                                    .WithTrait(new StackingBulkTrait<ItemReference>(5, 5))
            );
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkFloor2)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(BodySize.OneByOne, FloorLayer))
            );
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkFloor3)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(BodySize.OneByOne, FloorLayer))
            );
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItem1)
                                                    .WithTrait(new BulkItemGridPositionTrait<ItemReference>(BodySize.OneByOne, ItemLayer))
                                                    .WithTrait(new StackingBulkTrait<ItemReference>(5, 5))
            );
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItem2)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(BodySize.OneByOne, ItemLayer))
            );
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItem3)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(BodySize.OneByOne, ItemLayer))
            );
            ItemEntityContext.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ReferenceItem1)
                                                        .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(BodySize.OneByOne, ItemLayer))
            );

            ItemPlacementContext = new ItemPlacementServiceContext<ItemReference>()
                                   .WithLayer(FloorLayer,
                                              new ItemPlacementService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext),
                                              new ItemPlacementLocationService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext))
                                   .WithLayer(ItemLayer,
                                              new ItemPlacementService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext),
                                              new ItemPlacementLocationService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext));
            Items = new ItemFixture<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext, ItemPlacementContext);

            ActorMapContext = new DefaultMapContext<ActorReference>(DynamicDataViewConfiguration.Default16X16).WithBasicGridMapLayer(ActorLayer);
            ActorEntityContext = new ItemContextBackend<ActorReference>(new ActorReferenceMetaData());
            ActorEntityContext.EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ActorReference>>();
            ActorEntityContext.EntityRegistry.Register<DestroyedMarker>();
            ActorEntityContext.EntityRegistry.Register<CascadingDestroyedMarker>();
            ActorEntityContext.EntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            ActorEntityContext.ItemRegistry.Register(new ReferenceItemDeclaration<ActorReference>(Actor)
                                                         .WithTrait(new ReferenceItemGridPositionTrait<ActorReference>(BodySize.OneByOne, ActorLayer))
            );
            
            var actorPlacementContext = new ItemPlacementServiceContext<ActorReference>()
                                   .WithLayer(FloorLayer,
                                              new ItemPlacementService<ActorReference>(ActorEntityContext.ItemResolver, ActorMapContext),
                                              new ItemPlacementLocationService<ActorReference>(ActorEntityContext.ItemResolver, ActorMapContext))
                                   .WithLayer(ItemLayer,
                                              new ItemPlacementService<ActorReference>(ActorEntityContext.ItemResolver, ActorMapContext),
                                              new ItemPlacementLocationService<ActorReference>(ActorEntityContext.ItemResolver, ActorMapContext));
            Actors = new ItemFixture<ActorReference>(ActorEntityContext.ItemResolver, ActorMapContext, actorPlacementContext);

            ActorPlacementContext = new ItemPlacementServiceContext<ActorReference>()
                .WithLayer(ActorLayer,
                           new ItemPlacementService<ActorReference>(ActorEntityContext.ItemResolver, ActorMapContext),
                           new ItemPlacementLocationService<ActorReference>(ActorEntityContext.ItemResolver, ActorMapContext));

            MapBuilder = new MapBuilder();
            MapBuilder.WithLayer(FloorLayer, ItemEntityContext.ItemResolver, ItemMapContext, ItemPlacementContext);
            MapBuilder.WithLayer(ItemLayer, ItemEntityContext.ItemResolver, ItemMapContext, ItemPlacementContext);
            MapBuilder.WithLayer(ActorLayer, ActorEntityContext.ItemResolver, ActorMapContext, ActorPlacementContext);
        }

        public EntityContext<ItemFixture<ItemReference>, ItemReference> GivenAnItem(ItemDeclarationId item)
            => new EntityContext<ItemFixture<ItemReference>, ItemReference>(Items, item);

        public EntityContext<ItemFixture<ActorReference>, ActorReference> GivenAnActor(ItemDeclarationId item)
            => new EntityContext<ItemFixture<ActorReference>, ActorReference>(Actors, item);

        public EntityContext<ItemFixture<ItemReference>, ItemReference> GivenAnEmptyItem()
            => new EntityContext<ItemFixture<ItemReference>, ItemReference>(Items);

        public EntityContext<ItemFixture<ActorReference>, ActorReference> GivenAnEmptyActor()
            => new EntityContext<ItemFixture<ActorReference>, ActorReference>(Actors);
    }
}
