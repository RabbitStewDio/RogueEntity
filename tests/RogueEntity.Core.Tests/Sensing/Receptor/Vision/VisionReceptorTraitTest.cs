using EnTTSharp.Entities;
using NUnit.Framework;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Vision
{
    [TestFixture]
    public class VisionReceptorTraitTest: ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, SensoryReceptorData<VisionSense>, VisionSenseTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly VisionSenseReceptorPhysicsConfiguration physics;

        public VisionReceptorTraitTest()
        {
            physics = new VisionSenseReceptorPhysicsConfiguration(new LightPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev)));
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<LightSourceDefinition>();
            context.ActorEntityRegistry.RegisterNonConstructable<SenseSourceState<VisionSense>>();
            context.ActorEntityRegistry.RegisterFlag<ObservedSenseSource<VisionSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseDirtyFlag<VisionSense>>();
            
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorData<VisionSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorState<VisionSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<VisionSense, VisionSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<VisionSense>>();

            return context;
        }
        
        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override ItemRegistry<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override VisionSenseTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new VisionSenseTrait<SenseMappingTestContext, ActorReference>(physics, 1.9f);
        }

        protected override IItemComponentTestDataFactory<SensoryReceptorData<VisionSense>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<SensoryReceptorData<VisionSense>>(new SensoryReceptorData<VisionSense>(new SenseSourceDefinition(physics.VisionPhysics.DistanceMeasurement, 1.9f), true),
                                                                                      new SensoryReceptorData<VisionSense>(new SenseSourceDefinition(physics.VisionPhysics.DistanceMeasurement, 10), true),
                                                                                      new SensoryReceptorData<VisionSense>(new SenseSourceDefinition(physics.VisionPhysics.DistanceMeasurement, 12), false)
            ).WithRemoveProhibited();
        }
    }
}