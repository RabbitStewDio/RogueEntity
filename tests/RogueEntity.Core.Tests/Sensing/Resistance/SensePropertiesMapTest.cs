using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Tests.Fixtures;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Tests.Sensing.Resistance
{
    [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
    [TestFixture]
    public class SensePropertiesMapTest
    {
        static readonly ILogger logger = SLog.ForContext<SensePropertiesMapTest>();
        SenseMappingTestContext ctx;
        ItemDeclarationId wall;
        ItemDeclarationId ceilingFan;
        AggregatePropertiesLayer<float, SensoryResistance<VisionSense>> s;

        [SetUp]
        public void SetUp()
        {
            ctx = new SenseMappingTestContext();
            wall = ctx.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>("Wall", new WorldEntityTag("WallTag"))
                                                 .WithTrait(new SensoryResistanceTrait<ItemReference, VisionSense>(Percentage.Full))
            );
            ceilingFan = ctx.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>("Fan", new WorldEntityTag("FanTag"))
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
        public void TestMapUpdate()
        {
            ctx.TryGetItemGridDataFor(TestMapLayers.One, out var data).Should().BeTrue();
            var itemReference = ctx.ItemResolver.Instantiate(wall);
            data.TryInsertItem(itemReference, EntityGridPosition.Of(TestMapLayers.One, 0, 0)).Should().BeTrue();

            s.AddProcess(TestMapLayers.One, new SensePropertiesDataProcessor<ItemReference, VisionSense>(TestMapLayers.One, ctx,ctx.ItemResolver, 0, 0, 0, 64, 64));

            s.Process();
            s.ResetDirtyFlags();
            s.AggregatedView[0, 0].Should().Be(Percentage.Full);

            data.TryRemoveItem(itemReference, EntityGridPosition.Of(TestMapLayers.One, 0, 0)).Should().BeTrue();
            // without someone marking the data as dirty, no update will be done.
            logger.Debug("Updating map; but not fired event yet");            
            s.Process();
            s.AggregatedView[0, 0].Should().Be(Percentage.Full);

            // After marking the map dirty, the processor should now see the map contents and update accordingly
            s.MarkDirty(EntityGridPosition.Of(TestMapLayers.One, 0, 0));
            logger.Debug("Updating map after fired event");            
            s.Process();
            s.AggregatedView[0, 0].Should().Be(0);
        }

        [Test]
        public void TestCombinedMapProcessing()
        {
            ctx.TryGetItemGridDataFor(TestMapLayers.One, out var dataLayer1).Should().BeTrue();
            dataLayer1.TryInsertItem(ctx.ItemResolver.Instantiate( wall), EntityGridPosition.Of(TestMapLayers.One, 0, 0)).Should().BeTrue();

            ctx.TryGetItemGridDataFor(TestMapLayers.Two, out var dataLayer2).Should().BeTrue();
            dataLayer2.TryInsertItem(ctx.ItemResolver.Instantiate( ceilingFan), EntityGridPosition.Of(TestMapLayers.Two, 0, 0)).Should().BeTrue();

            s.AddProcess(TestMapLayers.One, new SensePropertiesDataProcessor<ItemReference, VisionSense>(TestMapLayers.One, ctx, ctx.ItemResolver,0, 0, 0, 64, 64));
            s.AddProcess(TestMapLayers.Two, new SensePropertiesDataProcessor<ItemReference, VisionSense>(TestMapLayers.Two, ctx, ctx.ItemResolver, 0, 0, 0, 64, 64));

            // After marking the map dirty, the processor should now see the map contents and update accordingly
            s.MarkDirty(EntityGridPosition.Of(TestMapLayers.Indeterminate, 0, 0));
            s.Process();
            s.AggregatedView[0, 0].Should().Be(Percentage.Full);
        }
    }
}