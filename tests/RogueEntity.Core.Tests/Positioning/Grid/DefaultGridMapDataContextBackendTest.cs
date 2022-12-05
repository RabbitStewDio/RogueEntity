using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Tests.Positioning.Grid
{
    [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
    public class DefaultGridMapDataContextBackendTest
    {
        IConfigurableMapContext<ItemReference> gdc;

        [SetUp]
        public void SetUp()
        {
            gdc = new DefaultMapContext<ItemReference>(DynamicDataViewConfiguration.Default32X32)
                .WithBasicGridMapLayer(TestMapLayers.One);
        }

        [Test]
        public void ValidateMapProperties()
        {
            gdc.Layers().Should().ContainInOrder(TestMapLayers.One);
            gdc.Config.OffsetX.Should().Be(DynamicDataViewConfiguration.Default32X32.OffsetX);
            gdc.Config.OffsetY.Should().Be(DynamicDataViewConfiguration.Default32X32.OffsetY);
            gdc.Config.TileSizeX.Should().Be(DynamicDataViewConfiguration.Default32X32.TileSizeX);
            gdc.Config.TileSizeY.Should().Be(DynamicDataViewConfiguration.Default32X32.TileSizeY);
            gdc.TryGetMapDataFor(TestMapLayers.One, out _).Should().BeTrue();
            gdc.TryGetMapDataFor(TestMapLayers.Two, out _).Should().BeFalse();
        }

        [Test]
        public void ValidateMapAccess()
        {
            gdc.TryGetMapDataFor(TestMapLayers.One, out var view).Should().BeTrue();
            
            view.TryInsertItem(ItemReference.FromBulkItem(1, 10), EntityGridPosition.Of(TestMapLayers.One, 0, 0)).Should().BeTrue();
            view.TryInsertItem(ItemReference.FromBulkItem(1, 40), EntityGridPosition.Of(TestMapLayers.One, 40, 40)).Should().BeTrue();

            view.At(0, 0).Should().Be(ItemReference.FromBulkItem(1, 10), "because we explicitly wrote here");
            view.At(40, 40).Should().Be(ItemReference.FromBulkItem(1, 40),"because we explicitly wrote here");

            view.At(0, 40).Should().Be(default(ItemReference), "because View chunk has not been created");
            view.At(40, 0).Should().Be(default(ItemReference), "because View chunk has not been created");
        }

        [Test]
        public void ValidateResetState()
        {
            gdc.TryGetMapDataFor(TestMapLayers.One, out var view).Should().BeTrue();
            
            view.TryInsertItem(ItemReference.FromBulkItem(1, 10), EntityGridPosition.Of(TestMapLayers.One, 0, 0)).Should().BeTrue();
            view.TryInsertItem(ItemReference.FromBulkItem(1, 40), EntityGridPosition.Of(TestMapLayers.One, 40, 40)).Should().BeTrue();

            var d = (IMapContextInitializer<ItemReference>)gdc;
            d.ResetState();

            view.At(0, 0).Should().Be(default(ItemReference), "because View chunk has not been created");
            view.At(40, 40).Should().Be(default(ItemReference), "because View chunk has not been created");
        }
    }
}
