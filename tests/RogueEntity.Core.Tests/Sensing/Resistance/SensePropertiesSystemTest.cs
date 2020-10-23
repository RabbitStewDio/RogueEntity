using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
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
        SensePropertiesSystem<SenseMappingTestContext> sps;
        
        [SetUp]
        public void SetUp()
        {
            ctx = new SenseMappingTestContext();
            wall = ctx.ItemRegistry.Register(new BulkItemDeclaration<SenseMappingTestContext, ItemReference>("Wall", "WallTag")
                                                 .WithTrait(new SensoryResistanceTrait<SenseMappingTestContext, ItemReference>(
                                                                Percentage.Full, Percentage.Of(0.5), Percentage.Empty, Percentage.Empty)));
            ceilingFan = ctx.ItemRegistry.Register(new BulkItemDeclaration<SenseMappingTestContext, ItemReference>("Fan", "FanTag")
                                                       .WithTrait(new SensoryResistanceTrait<SenseMappingTestContext, ItemReference>(
                                                                      Percentage.Empty, Percentage.Of(0.5), Percentage.Of(0.2f), Percentage.Of(0.5f))));
            sps = new SensePropertiesSystem<SenseMappingTestContext>(0, 0, 64, 64);
            sps.AddLayer<SenseMappingTestContext, ItemReference>(TestMapLayers.One);
            sps.AddLayer<SenseMappingTestContext, ItemReference>(TestMapLayers.Two);
        }

        [Test]
        public void Do()
        {
            sps.Start(ctx);
            sps.DefinedZLayers.Should().BeEmpty();
            sps.TryGet(0, out _).Should().BeFalse();
            
            ctx.TryGetGridDataFor(TestMapLayers.One, out IGridMapDataContext<SenseMappingTestContext, ItemReference> dataLayer1).Should().BeTrue();
            dataLayer1.TrySet(EntityGridPosition.Of(TestMapLayers.One, 0, 0, 0), ctx.ItemResolver.Instantiate(ctx, wall)).Should().BeTrue();

            ctx.TryGetGridDataFor(TestMapLayers.Two, out IGridMapDataContext<SenseMappingTestContext, ItemReference> dataLayer2).Should().BeTrue();
            dataLayer2.TrySet(EntityGridPosition.Of(TestMapLayers.Two, 0, 0, 0), ctx.ItemResolver.Instantiate(ctx, ceilingFan)).Should().BeTrue();

            sps.ProcessSenseProperties(ctx);
            sps.DefinedZLayers.Should().BeEquivalentTo(0);

            sps.TryGet(0, out var map).Should().BeTrue();
            map[0, 0].Should().Be(new SensoryResistance(Percentage.Full, Percentage.Full, Percentage.Of(0.2f), Percentage.Of(0.5f)));
            
            
            sps.Stop(ctx);
        }
    }
}