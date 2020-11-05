using EnTTSharp.Entities;
using NUnit.Framework;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Touch;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Touch;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Sources.Touch
{
    [TestFixture]
    public class TouchSourceTraitTest : ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, TouchSourceDefinition, TouchReceptorTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly TouchSenseReceptorPhysicsConfiguration physics;

        public TouchSourceTraitTest()
        {
            physics = new TouchSenseReceptorPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev));
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<TouchSourceDefinition>();
            context.ActorEntityRegistry.RegisterNonConstructable<SenseSourceState<TouchSense>>();
            context.ActorEntityRegistry.RegisterFlag<ObservedSenseSource<TouchSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseDirtyFlag<TouchSense>>();

            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorData<TouchSense, TouchSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorState<TouchSense, TouchSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TouchSense, TouchSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<TouchSense, TouchSense>>();
            return context;
        }

        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override ItemRegistry<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override TouchReceptorTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new TouchReceptorTrait<SenseMappingTestContext, ActorReference>(physics);
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