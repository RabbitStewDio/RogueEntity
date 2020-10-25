using NUnit.Framework;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources.Heat;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Sources.Heat
{
    [TestFixture]
    public class HeatSystemTest : SenseSystemTestBase<TemperatureSense, HeatSystem, HeatSourceDefinition>
    {
        const string EmptyRoom = @"
// 11x11; an empty room
1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 0, 0, 0, 0, 0, 0, 0, 1 
1, 1, 1, 1, 1, 1, 1, 1, 1 
";

        const string EmptyRoomResult = @"
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,277.000,277.000,277.000,277.000,277.000,277.000,276.000,275.000,274.000,273.000,272.000,271.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,278.000,278.000,278.000,278.000,278.000,277.000,276.000,275.000,274.000,273.000,272.000,271.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,278.000,279.000,279.000,279.000,278.000,277.000,276.000,275.000,274.000,273.000,272.000,271.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,278.000,279.000,280.000,279.000,278.000,277.000,276.000,275.000,274.000,273.000,272.000,271.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,278.000,279.000,279.000,279.000,278.000,277.000,276.000,275.000,274.000,273.000,272.000,271.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,278.000,278.000,278.000,278.000,278.000,277.000,276.000,275.000,274.000,273.000,272.000,271.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,277.000,277.000,277.000,277.000,277.000,277.000,276.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,276.000,276.000,276.000,276.000,276.000,276.000,276.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,275.000,275.000,275.000,275.000,275.000,275.000,275.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,274.000,274.000,274.000,274.000,274.000,274.000,274.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,273.000,273.000,273.000,273.000,273.000,273.000,273.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000
270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000,270.000";

        HeatPhysicsConfiguration lightPhysics;

        protected override SensoryResistance Convert(float f)
        {
            return new SensoryResistance(Percentage.Empty, Percentage.Empty, Percentage.Of(f), Percentage.Empty);
        }

        protected override HeatSystem CreateSystem()
        {
            return new HeatSystem(senseProperties.AsLazy<ISensePropertiesSource>(),
                                   senseCache.AsLazy<IGlobalSenseStateCacheProvider>(),
                                   timeSource.AsLazy<ITimeSource>(),
                                   senseCache,
                                   lightPhysics.CreateHeatPropagationAlgorithm(), lightPhysics);
        }

        protected override ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> AttachTrait(ReferenceItemDeclaration<SenseMappingTestContext, ItemReference> decl)
        {
            switch (decl.Id.Id)
            {
                case "SenseSource-Active-10":
                    decl.WithTrait(new HeatSourceTrait<SenseMappingTestContext, ItemReference>(lightPhysics, Temperature.FromCelsius(10)));
                    break;
                case "SenseSource-Active-5":
                    decl.WithTrait(new HeatSourceTrait<SenseMappingTestContext, ItemReference>(lightPhysics, Temperature.FromCelsius(5)));
                    break;
                case "SenseSource-Inactive-5":
                    decl.WithTrait(new HeatSourceTrait<SenseMappingTestContext, ItemReference>(lightPhysics));
                    break;
            }

            return decl;
        }

        [SetUp]
        public override void SetUp()
        {
            lightPhysics = new HeatPhysicsConfiguration(LinearDecaySensePhysics.For(DistanceCalculation.Chebyshev), Temperature.FromCelsius(0));
            base.SetUp();
        }

        [Test]
        [TestCase(nameof(EmptyRoom), EmptyRoom, EmptyRoomResult)]
        public void Do(string id, string sourceText, string expectedResultText)
        {
            base.PerformTest(id, sourceText, expectedResultText);
        }
    }
}