using NUnit.Framework;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Sources.Heat
{
    [TestFixture]
    public class HeatSourceTraitTest : ItemComponentTraitTestBase<ActorReference, HeatSourceDefinition, HeatSourceTrait<ActorReference>>
    {
        readonly HeatPhysicsConfiguration physics;

        public HeatSourceTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new HeatPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev), Temperature.FromCelsius(20));
        }

        protected override void SetUpPrepare()
        {
            EntityRegistry.RegisterNonConstructable<HeatSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<TemperatureSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<TemperatureSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<TemperatureSense>>();
        }

        protected override HeatSourceTrait<ActorReference> CreateTrait()
        {
            return new HeatSourceTrait<ActorReference>(physics);
        }

        protected override IItemComponentTestDataFactory<HeatSourceDefinition> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<HeatSourceDefinition>(
                default,
                new HeatSourceDefinition(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement,
                                                                   physics.HeatPhysics.AdjacencyRule,
                                                                   10), true),
                new HeatSourceDefinition(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement,
                                                                   physics.HeatPhysics.AdjacencyRule,
                                                                   12), false)
            );
        }
    }
}