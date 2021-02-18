using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Core.Tests.Positioning.Grid.GridItemPlacementServiceTest
{
    public class GridItemPlacementServiceFixture: WhenFixtureSupport, IItemFixture
    {
        protected MapLayer DefaultLayer = new MapLayer(1, "Default Layer");
        protected GridItemPlacementService<ItemReference> PlacementService;

        protected ItemContextBackend<ItemReference> ItemEntityContext;
        protected DefaultGridPositionContextBackend<ItemReference> ItemMapContext;
        protected ItemPlacementServiceContext<ItemReference> ItemPlacementContext;

        public static readonly ItemDeclarationId ReferenceItemA = "ReferenceItemA"; 
        public static readonly ItemDeclarationId ReferenceItemB = "ReferenceItemB"; 
        public static readonly ItemDeclarationId StackingBulkItemA = "StackingBulkItemA"; 
        public static readonly ItemDeclarationId StackingBulkItemB = "StackingBulkItemB"; 
        public static readonly ItemDeclarationId BulkItemC = "BulkItemC"; 
        
        [SetUp]
        public void SetUp()
        {
            ItemEntityContext = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            ItemEntityContext.EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            ItemEntityContext.EntityRegistry.RegisterNonConstructable<EntityGridPosition>();
            ItemEntityContext.EntityRegistry.RegisterNonConstructable<EntityGridPositionChangedMarker>();

            ItemMapContext = new DefaultGridPositionContextBackend<ItemReference>()
                .WithDefaultMapLayer(DefaultLayer);

            ItemPlacementContext = new ItemPlacementServiceContext<ItemReference>()
                .WithLayer(DefaultLayer,
                           new GridItemPlacementService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext),
                           new GridItemPlacementLocationService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext));

            ItemEntityContext.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ReferenceItemA).WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(DefaultLayer)));
            ItemEntityContext.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ReferenceItemB).WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(DefaultLayer)));
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(StackingBulkItemA)
                                                    .WithTrait(new BulkItemGridPositionTrait<ItemReference>(DefaultLayer))
                                                    .WithTrait(new StackingBulkTrait<ItemReference>(10)));
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(StackingBulkItemB)
                                                    .WithTrait(new BulkItemGridPositionTrait<ItemReference>(DefaultLayer))
                                                    .WithTrait(new StackingBulkTrait<ItemReference>(10)));
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItemC)
                                                    .WithTrait(new BulkItemGridPositionTrait<ItemReference>(DefaultLayer)));

            PlacementService = new GridItemPlacementService<ItemReference>(ItemEntityContext.ItemResolver, ItemMapContext);
        }

        IGridMapContext<ItemReference> IItemFixture.ItemMapContext => ItemMapContext;

        IItemResolver<ItemReference> IItemFixture.ItemResolver => ItemEntityContext.ItemResolver;

    }
}
