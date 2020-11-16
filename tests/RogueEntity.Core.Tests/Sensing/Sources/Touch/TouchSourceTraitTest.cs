using NUnit.Framework;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Touch;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Touch;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Sensing.Sources.Touch
{
    [TestFixture]
    public class TouchSourceTraitTest : ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, TouchSourceDefinition, TouchSenseTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly TouchSenseReceptorPhysicsConfiguration physics;

        public TouchSourceTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new TouchSenseReceptorPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev));
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            EntityRegistry.RegisterNonConstructable<TouchSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<TouchSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<TouchSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<TouchSense>>();

            EntityRegistry.RegisterNonConstructable<SensoryReceptorData<TouchSense, TouchSense>>();
            EntityRegistry.RegisterNonConstructable<SensoryReceptorState<TouchSense, TouchSense>>();
            EntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TouchSense, TouchSense>>();
            EntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<TouchSense, TouchSense>>();
            return context;
        }

        protected override TouchSenseTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new TouchSenseTrait<SenseMappingTestContext, ActorReference>(physics);
        }

        protected override IItemComponentTestDataFactory<TouchSourceDefinition> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            var distanceCalculation = physics.TouchPhysics.DistanceMeasurement;
            var intensity = distanceCalculation.MaximumStepDistance();
            return new ItemComponentTestDataFactory<TouchSourceDefinition>(
                new TouchSourceDefinition(new SenseSourceDefinition(distanceCalculation, 
                                                                    physics.TouchPhysics.AdjacencyRule,
                                                                    intensity), true),
                new TouchSourceDefinition(new SenseSourceDefinition(distanceCalculation, 
                                                                    physics.TouchPhysics.AdjacencyRule,
                                                                    intensity), false),
                new TouchSourceDefinition(new SenseSourceDefinition(distanceCalculation, 
                                                                    physics.TouchPhysics.AdjacencyRule,
                                                                    intensity), true)
            ).WithRemoveProhibited();
        }
    }
}