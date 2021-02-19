using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Sensing.Resistance
{
    public class SensePropertiesMapTest
    {
        SenseMappingTestContext ctx;
        ItemDeclarationId wall;
        ItemDeclarationId ceilingFan;
        AggregatePropertiesLayer<float, SensoryResistance<VisionSense>> s;

        [SetUp]
        public void SetUp()
        {
            ctx = new SenseMappingTestContext();
            wall = ctx.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>("Wall", "WallTag")
                                                 .WithTrait(new SensoryResistanceTrait<ItemReference, VisionSense>(Percentage.Full))
            );
            ceilingFan = ctx.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>("Fan", "FanTag")
                                                       .WithTrait(new SensoryResistanceTrait<ItemReference, VisionSense>(Percentage.Of(0.1)))
            );
            s = new AggregatePropertiesLayer<float, SensoryResistance<VisionSense>>(0, SensePropertiesSystem.ProcessTile, 0, 0, 64, 64);
        }

        [Test]
        public void TestMapCreation()
        {
            s.IsDefined(MapLayer.Indeterminate).Should().BeFalse();
            s.IsDefined(TestMapLayers.One).Should().BeFalse();
            s.IsDefined(TestMapLayers.Two).Should().BeFalse();
            s.AggregatedView[0, 0].Should().Be(0);

            s.AddProcess(TestMapLayers.One, new SensePropertiesDataProcessor<ItemReference, VisionSense>(TestMapLayers.One, ctx, ctx.ItemResolver, 0, 0, 0, 64, 64));
            s.IsDefined(TestMapLayers.One).Should().BeTrue();
            s.IsDefined(TestMapLayers.Two).Should().BeFalse();
            s.AggregatedView[0, 0].Should().Be(0);
        }

        [Test]
        public void TestMapProcessing()
        {
            ctx.TryGetItemGridDataFor(TestMapLayers.One, out var data).Should().BeTrue();
            data.TrySet(EntityGridPosition.Of(TestMapLayers.One, 0, 0), ctx.ItemResolver.Instantiate(wall)).Should().BeTrue();

            s.AddProcess(TestMapLayers.One, new SensePropertiesDataProcessor<ItemReference, VisionSense>(TestMapLayers.One, ctx,ctx.ItemResolver, 0, 0, 0, 64, 64));

            s.Process();
            s.ResetDirtyFlags();
            s.AggregatedView[0, 0].Should().Be(Percentage.Full);

            data.TrySet(EntityGridPosition.Of(TestMapLayers.One, 0, 0), default).Should().BeTrue();
            // without someone marking the data as dirty, no update will be done.
            s.Process();
            s.AggregatedView[0, 0].Should().Be(Percentage.Full);

            // After marking the map dirty, the processor should now see the map contents and update accordingly
            s.MarkDirty(EntityGridPosition.Of(TestMapLayers.One, 0, 0));
            s.Process();
            s.AggregatedView[0, 0].Should().Be(0);
        }

        [Test]
        public void TestCombinedMapProcessing()
        {
            ctx.TryGetItemGridDataFor(TestMapLayers.One, out var dataLayer1).Should().BeTrue();
            dataLayer1.TrySet(EntityGridPosition.Of(TestMapLayers.One, 0, 0), ctx.ItemResolver.Instantiate( wall)).Should().BeTrue();

            ctx.TryGetItemGridDataFor(TestMapLayers.Two, out var dataLayer2).Should().BeTrue();
            dataLayer2.TrySet(EntityGridPosition.Of(TestMapLayers.Two, 0, 0), ctx.ItemResolver.Instantiate( ceilingFan)).Should().BeTrue();

            s.AddProcess(TestMapLayers.One, new SensePropertiesDataProcessor<ItemReference, VisionSense>(TestMapLayers.One, ctx, ctx.ItemResolver,0, 0, 0, 64, 64));
            s.AddProcess(TestMapLayers.Two, new SensePropertiesDataProcessor<ItemReference, VisionSense>(TestMapLayers.Two, ctx, ctx.ItemResolver, 0, 0, 0, 64, 64));

            // After marking the map dirty, the processor should now see the map contents and update accordingly
            s.MarkDirty(EntityGridPosition.Of(TestMapLayers.Indeterminate, 0, 0));
            s.Process();
            s.AggregatedView[0, 0].Should().Be(Percentage.Full);
        }
    }
}