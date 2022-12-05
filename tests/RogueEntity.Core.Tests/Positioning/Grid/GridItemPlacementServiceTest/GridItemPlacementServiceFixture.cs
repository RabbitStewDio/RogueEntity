using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Tests.Positioning.Grid.GridItemPlacementServiceTest
{
    [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
    public class GridItemPlacementServiceFixture<TSelf> : ItemTestFixtureBase<TSelf, ItemReference>,
                                                          ITestFixture<TSelf, ItemReference>
        where TSelf : ItemTestFixtureBase<TSelf, ItemReference>, ITestFixture<TSelf, ItemReference>
    {
        protected MapLayer DefaultLayer = new MapLayer(1, "Default Layer");
        protected ItemPlacementService<ItemReference> PlacementService;

        protected ItemContextBackend<ItemReference> ItemEntityContext;
        protected IConfigurableMapContext<ItemReference> GridMapContext;

        public static readonly ItemDeclarationId ReferenceItemA = "ReferenceItemA";
        public static readonly ItemDeclarationId ReferenceItemB = "ReferenceItemB";
        public static readonly ItemDeclarationId StackingBulkItemA = "StackingBulkItemA";
        public static readonly ItemDeclarationId StackingBulkItemB = "StackingBulkItemB";
        public static readonly ItemDeclarationId BulkItemC = "BulkItemC";

        public EntityContext<TSelf, ItemReference> With(ItemReference r)
        {
            TSelf me = (TSelf)((object)this);
            return new EntityContext<TSelf, ItemReference>(me, r);
        }
        
        public EntityContext<TSelf, ItemReference> With(ItemDeclarationId r)
        {
            TSelf me = (TSelf)((object)this);
            return new EntityContext<TSelf, ItemReference>(me, r);
        }
        
        [SetUp]
        public void SetUp()
        {
            ItemEntityContext = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            ItemEntityContext.EntityRegistry.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            ItemEntityContext.EntityRegistry.RegisterNonConstructable<EntityGridPosition>();

            GridMapContext = new DefaultMapContext<ItemReference>(DynamicDataViewConfiguration.Default16X16)
                .WithBasicGridMapLayer(DefaultLayer);

            itemPlacementContext = new ItemPlacementServiceContext<ItemReference>()
                .WithLayer(DefaultLayer,
                           new ItemPlacementService<ItemReference>(ItemEntityContext.ItemResolver, GridMapContext),
                           new ItemPlacementLocationService<ItemReference>(ItemEntityContext.ItemResolver, GridMapContext));

            ItemEntityContext.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ReferenceItemA)
                                                        .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(BodySize.OneByOne, DefaultLayer)));
            ItemEntityContext.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ReferenceItemB)
                                                        .WithTrait(new ReferenceItemGridPositionTrait<ItemReference>(BodySize.OneByOne, DefaultLayer)));
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(StackingBulkItemA)
                                                    .WithTrait(new BulkItemGridPositionTrait<ItemReference>(BodySize.OneByOne, DefaultLayer))
                                                    .WithTrait(new StackingBulkTrait<ItemReference>(10)));
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(StackingBulkItemB)
                                                    .WithTrait(new BulkItemGridPositionTrait<ItemReference>(BodySize.OneByOne, DefaultLayer))
                                                    .WithTrait(new StackingBulkTrait<ItemReference>(10)));
            ItemEntityContext.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkItemC)
                                                        .WithTrait(new BulkItemGridPositionTrait<ItemReference>(BodySize.OneByOne, DefaultLayer)));

            PlacementService = new ItemPlacementService<ItemReference>(ItemEntityContext.ItemResolver, GridMapContext);
        }

        public override IItemResolver<ItemReference> ItemResolver => ItemEntityContext.ItemResolver;
        public override IMapContext<ItemReference> ItemMapContext => GridMapContext;
        
        ItemPlacementServiceContext<ItemReference> itemPlacementContext;
        public override IItemPlacementServiceContext<ItemReference> ItemPlacementContext => itemPlacementContext;
    }
}