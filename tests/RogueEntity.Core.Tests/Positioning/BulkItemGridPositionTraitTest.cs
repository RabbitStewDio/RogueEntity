using EnTTSharp.Entities;
using FluentAssertions;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Positioning
{
    public class BulkItemGridPositionTraitTest: ItemComponentTraitTestBase<TestGridPositionContext, ItemReference, EntityGridPosition, BulkItemGridPositionTrait<TestGridPositionContext, ItemReference>>
    {
        protected override IBulkDataStorageMetaData<ItemReference> ItemIdMetaData => new ItemReferenceMetaData();
        readonly MapLayer itemLayer;

        public BulkItemGridPositionTraitTest()
        {
            itemLayer = new MapLayer(1, "ItemLayer");
        }

        public override IItemComponentTestDataFactory<EntityGridPosition> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<EntityGridPosition>(Optional.Empty<EntityGridPosition>(),
                                                                        EntityGridPosition.Of(itemLayer, 10, 10, 10),
                                                                        EntityGridPosition.Of(itemLayer, 10, 20, 10))
                   .WithInvalidResult(EntityGridPosition.Of(itemLayer, 1000, 1000, 10)) // Given map is only 100x100 cells
                   .WithRemovedResult(Optional.Empty<EntityGridPosition>())
                ;
        }

        protected override EntityRegistry<ItemReference> EntityRegistry => Context.EntityRegistry;
        protected override ItemRegistry<TestGridPositionContext, ItemReference> ItemRegistry => Context.ItemRegistry;

        protected override TestGridPositionContext CreateContext()
        {
            return new TestGridPositionContext().WithMapLayer(itemLayer, new OnDemandGridMapDataContext<TestGridPositionContext, ItemReference>(100, 100));
        }

        protected override BulkItemGridPositionTrait<TestGridPositionContext, ItemReference> CreateTrait()
        {
            return new BulkItemGridPositionTrait<TestGridPositionContext, ItemReference>(Context.ItemResolver, itemLayer);
        }

        protected override void Validate_Apply(ItemDeclarationId itemId)
        {
            var item = Context.ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));
            if (testData.UpdateAllowed)
            {
                Context.ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeTrue($"because {item} has been successfully updated.");
                Context.ItemResolver.TryQueryData(item, Context, out EntityGridPosition _).Should().BeFalse();
                QueryMapData(testData.ChangedValue).Should().Be(item);
            }

            Context.ItemResolver.Apply(item, Context);

            testData.TryGetApplyValue(out _).Should().BeTrue();
            QueryMapData(testData.ChangedValue).Should().Be(item, "because apply should not reset existing data.");
        }

        protected override void Validate_Update(ItemDeclarationId itemId)
        {
            var item = Context.ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            testData.UpdateAllowed.Should().BeTrue();

            // We can write the data
            Context.ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeTrue();
            // but we cannot query it. Bulk items do not have entity system data stores, so they
            // only exist implicitly inside the map itself.
            Context.ItemResolver.TryQueryData(item, Context, out EntityGridPosition _).Should().BeFalse();
            QueryMapData(testData.ChangedValue).Should().Be(item);

            if (testData.TryGetInvalid(out var invalid))
            {
                Context.ItemResolver.TryUpdateData(item, Context, invalid, out item).Should().BeFalse();
                QueryMapData(testData.ChangedValue).Should().Be(item);
            }
        }

        ItemReference QueryMapData(EntityGridPosition pos)
        {
            if (pos.IsInvalid) return default;
            
            Context.TryGetGridDataFor(itemLayer, out var mapLayerData).Should().BeTrue();
            mapLayerData.TryGetMap(pos.GridZ, out var mapData).Should().BeTrue();
            return mapData[pos.GridX, pos.GridY];
        }
    }
}