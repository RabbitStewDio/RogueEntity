using NUnit.Framework;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Sources.Light
{
    [TestFixture]
    public class LightSourceTraitTest : ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, LightSourceDefinition, LightSourceTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly LightPhysicsConfiguration physics;

        public LightSourceTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new LightPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev));
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            EntityRegistry.RegisterNonConstructable<LightSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<VisionSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<VisionSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<VisionSense>>();
            return context;
        }

        protected override LightSourceTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new LightSourceTrait<SenseMappingTestContext, ActorReference>(physics, 0.5f, 0.3f, 1.9f, true);
        }

        protected override IItemComponentTestDataFactory<LightSourceDefinition> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<LightSourceDefinition>(
                new LightSourceDefinition(new SenseSourceDefinition(physics.LightPhysics.DistanceMeasurement, 
                                                                    physics.LightPhysics.AdjacencyRule,
                                                                    1.9f), 0.5f, 0.3f),
                new LightSourceDefinition(new SenseSourceDefinition(physics.LightPhysics.DistanceMeasurement, 
                                                                    physics.LightPhysics.AdjacencyRule,
                                                                    10), 0.1f, 1f),
                new LightSourceDefinition(new SenseSourceDefinition(physics.LightPhysics.DistanceMeasurement, 
                                                                    physics.LightPhysics.AdjacencyRule,
                                                                    12), 0.25f, 0.33f, false)
            );
        }
    }
}