using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Positioning.Grid
{
    public class DefaultGridMapDataContextBackendTest
    {
        DefaultGridPositionContextBackend<byte> gdc;

        [SetUp]
        public void SetUp()
        {
            gdc = new DefaultGridPositionContextBackend<byte>()
                .WithDefaultMapLayer(TestMapLayers.One);
        }

        [Test]
        public void ValidateMapProperties()
        {
            gdc.GridLayers().Should().ContainInOrder(TestMapLayers.One);
            gdc.OffsetX.Should().Be(DynamicDataViewConfiguration.Default_32x32.OffsetX);
            gdc.OffsetY.Should().Be(DynamicDataViewConfiguration.Default_32x32.OffsetY);
            gdc.TileSizeX.Should().Be(DynamicDataViewConfiguration.Default_32x32.TileSizeX);
            gdc.TileSizeY.Should().Be(DynamicDataViewConfiguration.Default_32x32.TileSizeY);
            gdc.TryGetGridDataFor(TestMapLayers.One, out _).Should().BeTrue();
            gdc.TryGetGridDataFor(TestMapLayers.Two, out _).Should().BeFalse();
        }

        [Test]
        public void ValidateMapAccess()
        {
            gdc.TryGetGridDataFor(TestMapLayers.One, out var dataContext).Should().BeTrue();
            dataContext.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing).Should().BeTrue();
            view[0, 0] = 10;
            view[40, 40] = 40;

            view.TryGet(0, 0, out var expected00).Should().BeTrue("because we explicitly wrote here");
            expected00.Should().Be(10);

            view.TryGet(40, 40, out var expected40).Should().BeTrue("because we explicitly wrote here");
            expected40.Should().Be(40);

            view.TryGet(0, 40, out _).Should().BeFalse("because View chunk has not been created");
            view.TryGet(40, 0, out _).Should().BeFalse("because View chunk has not been created");
        }

        [Test]
        public void ValidateResetState()
        {
            gdc.TryGetGridDataFor(TestMapLayers.One, out var dataContext).Should().BeTrue();
            dataContext.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing).Should().BeTrue();
            view[0, 0] = 10;
            view[40, 40] = 40;

            gdc.ResetState();

            view.TryGet(0, 0, out _).Should().BeFalse("because View chunk has not been reset");
            view.TryGet(40, 40, out _).Should().BeFalse("because View chunk has not been reset");
        }
    }
}
