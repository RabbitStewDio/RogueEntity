using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Touch;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Touch;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Touch
{
    public class TouchReceptorTraitTest : ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, SensoryReceptorData<TouchSense>,
        TouchReceptorTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly TouchSenseReceptorPhysicsConfiguration physics;

        public TouchReceptorTraitTest()
        {
            physics = new TouchSenseReceptorPhysicsConfiguration(
                new TouchPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev)),
                new FloodFillWorkingDataSource()
            );
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<TouchSourceDefinition>();
            context.ActorEntityRegistry.RegisterNonConstructable<SenseSourceState<TouchSense>>();
            context.ActorEntityRegistry.RegisterFlag<ObservedSenseSource<TouchSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseDirtyFlag<TouchSense>>();

            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorData<TouchSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorState<TouchSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TouchSense, TouchSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<TouchSense>>();

            return context;
        }

        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override ItemRegistry<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override TouchReceptorTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new TouchReceptorTrait<SenseMappingTestContext, ActorReference>(physics);
        }

        protected override IItemComponentTestDataFactory<SensoryReceptorData<TouchSense>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            var distanceCalculation = physics.TouchPhysics.DistanceMeasurement;
            var intensity = distanceCalculation.MaximumStepDistance();
            return new ItemComponentTestDataFactory<SensoryReceptorData<TouchSense>>(
                new SensoryReceptorData<TouchSense>(new SenseSourceDefinition(physics.TouchPhysics.DistanceMeasurement, intensity), true),
                new SensoryReceptorData<TouchSense>(new SenseSourceDefinition(physics.TouchPhysics.DistanceMeasurement, intensity), true),
                new SensoryReceptorData<TouchSense>(new SenseSourceDefinition(physics.TouchPhysics.DistanceMeasurement, intensity), false)
            ).WithRemoveProhibited();
        }
    }
}