using EnTTSharp.Entities;
using NUnit.Framework;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Sources.Heat
{
    [TestFixture]
    public class HeatSourceTraitTest : ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, HeatSourceDefinition, HeatSourceTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly HeatPhysicsConfiguration physics;

        public HeatSourceTraitTest()
        {
            physics = new HeatPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev), Temperature.FromCelsius(20));
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<HeatSourceDefinition>();
            context.ActorEntityRegistry.RegisterNonConstructable<SenseSourceState<TemperatureSense>>();
            context.ActorEntityRegistry.RegisterFlag<ObservedSenseSource<TemperatureSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseDirtyFlag<TemperatureSense>>();
            return context;
        }

        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override ItemRegistry<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override HeatSourceTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new HeatSourceTrait<SenseMappingTestContext, ActorReference>(physics);
        }

        protected override IItemComponentTestDataFactory<HeatSourceDefinition> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<HeatSourceDefinition>(default,
                                                                          new HeatSourceDefinition(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement, 10), true),
                                                                          new HeatSourceDefinition(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement, 12), false)
            );
        }
    }
}