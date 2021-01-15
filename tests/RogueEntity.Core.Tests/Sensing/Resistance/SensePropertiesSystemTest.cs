using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Sensing.Resistance
{
    public class SensePropertiesSystemTest
    {
        SenseMappingTestContext ctx;
        ItemDeclarationId wall;
        ItemDeclarationId ceilingFan;
        SensePropertiesSystem<VisionSense> sps;

        [SetUp]
        public void SetUp()
        {
            ctx = new SenseMappingTestContext();
            wall = ctx.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>("Wall", "WallTag")
                                                 .WithTrait(new SensoryResistanceTrait<ItemReference, VisionSense>(Percentage.Full)));
            ceilingFan = ctx.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>("Fan", "FanTag")
                                                       .WithTrait(new SensoryResistanceTrait<ItemReference, VisionSense>(Percentage.Of(0.1f))));
            sps = new SensePropertiesSystem<VisionSense>(0, 0, 64, 64);
            sps.AddLayer(ctx, ctx.ItemResolver, TestMapLayers.One);
            sps.AddLayer(ctx, ctx.ItemResolver, TestMapLayers.Two);
        }

        [Test]
        public void Do()
        {
            sps.Start();
            sps.GetActiveLayers().Should().BeEmpty();
            sps.TryGetView(0, out _).Should().BeFalse();

            ctx.TryGetItemGridDataFor(TestMapLayers.One, out var dataLayer1).Should().BeTrue();
            dataLayer1.TrySet(EntityGridPosition.Of(TestMapLayers.One, 0, 0), ctx.ItemResolver.Instantiate( wall)).Should().BeTrue();

            ctx.TryGetItemGridDataFor(TestMapLayers.Two, out var dataLayer2).Should().BeTrue();
            dataLayer2.TrySet(EntityGridPosition.Of(TestMapLayers.Two, 0, 0), ctx.ItemResolver.Instantiate( ceilingFan)).Should().BeTrue();

            sps.ProcessSenseProperties();
            sps.GetActiveLayers().Should().BeEquivalentTo(0);
           
            sps.TryGetView(0, out var map).Should().BeTrue();
            map[0, 0].Should().Be(Percentage.Full);


            sps.Stop();
        }
    }
}