﻿using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Positioning
{
    [TestFixture]
    public class ReferenceItemGridPositionTraitTest : ItemComponentTraitTestBase<TestGridPositionContext,
        ItemReference,
        EntityGridPosition,
        ReferenceItemGridPositionTrait<TestGridPositionContext, ItemReference>>
    {
        readonly MapLayer itemLayer;

        public ReferenceItemGridPositionTraitTest(): base(new ItemReferenceMetaData())
        {
            itemLayer = new MapLayer(1, "ItemLayer");
        }

        protected override IItemComponentTestDataFactory<EntityGridPosition> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<EntityGridPosition>(Optional.Empty<EntityGridPosition>(),
                                                                        EntityGridPosition.Of(itemLayer, 10, 10, 10),
                                                                        EntityGridPosition.Of(itemLayer, 10, 20, 10))
                   .WithRemovedResult(Optional.Empty<EntityGridPosition>())
                ;
        }

        protected override TestGridPositionContext CreateContext()
        {
            return new TestGridPositionContext().WithMapLayer(itemLayer, new DefaultGridMapDataContext<ItemReference>(itemLayer, 100, 100));
        }

        protected override ReferenceItemGridPositionTrait<TestGridPositionContext, ItemReference> CreateTrait()
        {
            return new ReferenceItemGridPositionTrait<TestGridPositionContext, ItemReference>(ItemResolver, Context, itemLayer);
        }

        protected override void Validate_Update(ItemDeclarationId itemId)
        {
            var item = ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            testData.UpdateAllowed.Should().BeTrue();
            ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeTrue();

            ItemResolver.TryQueryData(item, Context, out EntityGridPosition data).Should().BeTrue();
            data.Should().Be(testData.ChangedValue);
            QueryMapData(testData.ChangedValue).Should().Be(item);
            
            ItemResolver.TryUpdateData(item, Context, testData.OtherChangedValue, out item).Should().BeTrue();
            QueryMapData(testData.ChangedValue).Should().Be(ItemReference.Empty);
            QueryMapData(testData.OtherChangedValue).Should().Be(item);
            
            if (testData.TryGetInvalid(out var invalid))
            {
                ItemResolver.TryUpdateData(item, Context, invalid, out item).Should().BeFalse();

                ItemResolver.TryQueryData(item, Context, out EntityGridPosition data2).Should().BeTrue();
                data2.Should().Be(testData.OtherChangedValue);
            }
        }

        [Test]
        public void Validate_NotOverwriting_Existing_Data()
        {
            var item = ItemResolver.Instantiate(Context, ReferenceItemId);
            var testData = ProduceTestData(ProduceItemRelations(item));

            Context.TryGetGridDataFor(itemLayer, out var mapLayerData).Should().BeTrue();
            mapLayerData.TryGetWritableView(10, out var mapData, DataViewCreateMode.CreateMissing).Should().BeTrue();
            mapData[testData.ChangedValue.GridX, testData.ChangedValue.GridY] = ItemReference.FromBulkItem(1, 1); // write a dummy item into the map

            ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeFalse();
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
            var item = ItemResolver.Instantiate(Context, itemId);
            var testData = ProduceTestData(ProduceItemRelations(item));
            
            ItemResolver.TryRemoveData<EntityGridPosition>(item, Context, out item).Should().BeTrue();
            ItemResolver.TryQueryData(item, Context, out EntityGridPosition _).Should().BeFalse();

            ItemResolver.TryUpdateData(item, Context, testData.ChangedValue, out item).Should().BeTrue();
            ItemResolver.TryRemoveData<EntityGridPosition>(item, Context, out item).Should().BeTrue();
            QueryMapData(testData.ChangedValue).Should().Be(ItemReference.Empty);
        }

    }
}