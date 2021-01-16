using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Positioning.Grid
{
    [TestFixture]
    public class ReferenceItemGridPositionTraitTest : ItemComponentTraitTestBase<ItemReference, EntityGridPosition, ReferenceItemGridPositionTrait<ItemReference>>
    {
        readonly MapLayer itemLayer;

        public ReferenceItemGridPositionTraitTest() : base(new ItemReferenceMetaData())
        {
            itemLayer = new MapLayer(1, "ItemLayer");
        }

        protected override IItemComponentTestDataFactory<EntityGridPosition> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<EntityGridPosition>(EntityGridPosition.Invalid,
                                                                        EntityGridPosition.Of(itemLayer, 10, 10, 10),
                                                                        EntityGridPosition.Of(itemLayer, 10, 20, 10))
                    .WithRemovedResult(EntityGridPosition.Invalid)
                ;
        }

        protected override void SetUpPrepare()
        {
            Context = new TestGridPositionContext().WithMapLayer(itemLayer, new DefaultGridMapDataContext<ItemReference>(itemLayer, 100, 100));
        }

        public TestGridPositionContext Context { get; set; }

        protected override ReferenceItemGridPositionTrait<ItemReference> CreateTrait()
        {
            return new ReferenceItemGridPositionTrait<ItemReference>(ItemResolver, Context, itemLayer);
        }

        protected override void Validate_Update(ItemDeclarationId itemId)
        {
            var item = ItemResolver.Instantiate(itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            testData.UpdateAllowed.Should().BeTrue();
            ItemResolver.TryUpdateData(item, testData.ChangedValue, out item).Should().BeTrue();

            ItemResolver.TryQueryData(item, out EntityGridPosition data).Should().BeTrue();
            data.Should().Be(testData.ChangedValue);
            QueryMapData(testData.ChangedValue).Should().Be(item);

            ItemResolver.TryUpdateData(item, testData.OtherChangedValue, out item).Should().BeTrue();
            QueryMapData(testData.ChangedValue).Should().Be(ItemReference.Empty);
            QueryMapData(testData.OtherChangedValue).Should().Be(item);

            if (testData.TryGetInvalid(out var invalid))
            {
                ItemResolver.TryUpdateData(item, invalid, out item).Should().BeFalse();

                ItemResolver.TryQueryData(item, out EntityGridPosition data2).Should().BeTrue();
                data2.Should().Be(testData.OtherChangedValue);
            }
        }

        [Test]
        public void Validate_NotOverwriting_Existing_Data()
        {
            var item = ItemResolver.Instantiate(ReferenceItemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            Context.TryGetGridDataFor(itemLayer, out var mapLayerData).Should().BeTrue();
            mapLayerData.TryGetWritableView(10, out var mapData, DataViewCreateMode.CreateMissing).Should().BeTrue();
            mapData[testData.ChangedValue.GridX, testData.ChangedValue.GridY] = ItemReference.FromBulkItem(1, 1); // write a dummy item into the map

            ItemResolver.TryUpdateData(item, testData.ChangedValue, out item).Should().BeFalse();
        }


        ItemReference QueryMapData(EntityGridPosition pos)
        {
            if (pos.IsInvalid) return default;

            Context.TryGetGridDataFor(itemLayer, out var mapLayerData).Should().BeTrue();
            mapLayerData.TryGetWritableView(pos.GridZ, out var mapData).Should().BeTrue();
            return mapData[pos.GridX, pos.GridY];
        }

        protected override void Validate_Remove(ItemDeclarationId itemId)
        {
            var item = ItemResolver.Instantiate(itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            ItemResolver.TryRemoveData<EntityGridPosition>(item, out item).Should().BeTrue();
            ItemResolver.TryQueryData(item, out EntityGridPosition p).Should().BeTrue();
            p.Should().Be(EntityGridPosition.Invalid);

            ItemResolver.TryUpdateData(item, testData.ChangedValue, out item).Should().BeTrue();
            ItemResolver.TryRemoveData<EntityGridPosition>(item, out item).Should().BeTrue();
            QueryMapData(testData.ChangedValue).Should().Be(ItemReference.Empty);
        }
    }
}
