using FluentAssertions;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Positioning.Grid
{
    public class BulkItemGridPositionTraitTest: ItemComponentTraitTestBase<ItemReference, EntityGridPosition, BulkItemGridPositionTrait<ItemReference>>
    {
        readonly MapLayer itemLayer;

        public BulkItemGridPositionTraitTest(): base(new ItemReferenceMetaData())
        {
            itemLayer = new MapLayer(1, "ItemLayer");
        }

        protected override IItemComponentTestDataFactory<EntityGridPosition> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<EntityGridPosition>(Optional.Empty(),
                                                                        EntityGridPosition.Of(itemLayer, 10, 10, 10),
                                                                        EntityGridPosition.Of(itemLayer, 10, 20, 10))
                   .WithRemovedResult(Optional.Empty())
                ;
        }

        protected override void SetUpPrepare()
        {
            Context = new TestGridPositionContext().WithMapLayer(itemLayer, new DefaultGridMapDataContext<ItemReference>(itemLayer, 100, 100));
            
        }

        public TestGridPositionContext Context { get; set; }

        protected override BulkItemGridPositionTrait<ItemReference> CreateTrait()
        {
            return new BulkItemGridPositionTrait<ItemReference>(new ItemReferenceMetaData(), ItemResolver, Context, itemLayer);
        }

        protected override void Validate_Apply(ItemDeclarationId itemId)
        {
            var item = ItemResolver.Instantiate(itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));
            if (testData.UpdateAllowed)
            {
                ItemResolver.TryUpdateData(item, testData.ChangedValue, out item).Should().BeTrue($"because {item} has been successfully updated.");
                ItemResolver.TryQueryData(item, out EntityGridPosition _).Should().BeFalse();
                QueryMapData(testData.ChangedValue).Should().Be(item);
            }

            ItemResolver.Apply(item);

            testData.TryGetApplyValue(out _).Should().BeTrue();
            QueryMapData(testData.ChangedValue).Should().Be(item, "because apply should not reset existing data.");
        }

        protected override void Validate_Update(ItemDeclarationId itemId)
        {
            var item = ItemResolver.Instantiate(itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            testData.UpdateAllowed.Should().BeTrue();

            // We can write the data
            ItemResolver.TryUpdateData(item, testData.ChangedValue, out item).Should().BeTrue();
            // but we cannot query it. Bulk items do not have entity system data stores, so they
            // only exist implicitly inside the map itself.
            ItemResolver.TryQueryData(item, out EntityGridPosition _).Should().BeFalse();
            QueryMapData(testData.ChangedValue).Should().Be(item);

            if (testData.TryGetInvalid(out var invalid))
            {
                ItemResolver.TryUpdateData(item, invalid, out item).Should().BeFalse();
                QueryMapData(testData.ChangedValue).Should().Be(item);
            }
        }

        ItemReference QueryMapData(EntityGridPosition pos)
        {
            if (pos.IsInvalid) return default;
            
            Context.TryGetGridDataFor(itemLayer, out var mapLayerData).Should().BeTrue();
            mapLayerData.TryGetView(pos.GridZ, out var mapData).Should().BeTrue();
            return mapData[pos.GridX, pos.GridY];
        }
    }
}