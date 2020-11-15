using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Sensing.Resistance
{
    public class SensePropertiesSystemTest
    {
        SenseMappingTestContext ctx;
        ItemDeclarationId wall;
        ItemDeclarationId ceilingFan;
        SensePropertiesSystem<SenseMappingTestContext, VisionSense> sps;

        [SetUp]
        public void SetUp()
        {
            ctx = new SenseMappingTestContext();
            wall = ctx.ItemRegistry.Register(new BulkItemDeclaration<SenseMappingTestContext, ItemReference>("Wall", "WallTag")
                                                 .WithTrait(new SensoryResistanceTrait<SenseMappingTestContext, ItemReference, VisionSense>(Percentage.Full)));
            ceilingFan = ctx.ItemRegistry.Register(new BulkItemDeclaration<SenseMappingTestContext, ItemReference>("Fan", "FanTag")
                                                       .WithTrait(new SensoryResistanceTrait<SenseMappingTestContext, ItemReference, VisionSense>(Percentage.Of(0.1f))));
            sps = new SensePropertiesSystem<SenseMappingTestContext, VisionSense>(0, 0, 64, 64);
            sps.AddLayer<SenseMappingTestContext, ItemReference, VisionSense>(ctx, ctx.ItemResolver, TestMapLayers.One);
            sps.AddLayer<SenseMappingTestContext, ItemReference, VisionSense>(ctx, ctx.ItemResolver, TestMapLayers.Two);
        }

        [Test]
        public void Do()
        {
            sps.Start(ctx);
            sps.GetActiveLayers().Should().BeEmpty();
            sps.TryGetView(0, out _).Should().BeFalse();

            ctx.TryGetItemGridDataFor(TestMapLayers.One, out var dataLayer1).Should().BeTrue();
            dataLayer1.TrySet(EntityGridPosition.Of(TestMapLayers.One, 0, 0), ctx.ItemResolver.Instantiate(ctx, wall)).Should().BeTrue();

            ctx.TryGetItemGridDataFor(TestMapLayers.Two, out var dataLayer2).Should().BeTrue();
            dataLayer2.TrySet(EntityGridPosition.Of(TestMapLayers.Two, 0, 0), ctx.ItemResolver.Instantiate(ctx, ceilingFan)).Should().BeTrue();

            sps.ProcessSenseProperties(ctx);
            sps.GetActiveLayers().Should().BeEquivalentTo(0);
           
            sps.TryGetView(0, out var map).Should().BeTrue();
            map[0, 0].Should().Be(new SensoryResistance<VisionSense>(Percentage.Full));


            sps.Stop(ctx);
        }
    }
}