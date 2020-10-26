using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Receptors.Smell;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Sensing.Sources.Smell;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Smell
{
    public class SmellReceptorTraitTest: ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, SensoryReceptorData<SmellSense>, SmellDirectionSenseTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly SmellPhysicsConfiguration physics;

        public SmellReceptorTraitTest()
        {
            physics = new SmellPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev));
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<SmellSourceDefinition>();
            context.ActorEntityRegistry.RegisterNonConstructable<SenseSourceState<SmellSense>>();
            context.ActorEntityRegistry.RegisterFlag<ObservedSenseSource<SmellSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseDirtyFlag<SmellSense>>();
            
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorData<SmellSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorState<SmellSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<SmellSense, SmellSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<SmellSense>>();

            return context;
        }
        
        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override ItemRegistry<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override SmellDirectionSenseTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new SmellDirectionSenseTrait<SenseMappingTestContext, ActorReference>(physics, 1.9f);
        }

        protected override IItemComponentTestDataFactory<SensoryReceptorData<SmellSense>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<SensoryReceptorData<SmellSense>>(new SensoryReceptorData<SmellSense>(new SenseSourceDefinition(physics.SmellPhysics.DistanceMeasurement, 1.9f), true),
                                                                                     new SensoryReceptorData<SmellSense>(new SenseSourceDefinition(physics.SmellPhysics.DistanceMeasurement, 10), true),
                                                                                     new SensoryReceptorData<SmellSense>(new SenseSourceDefinition(physics.SmellPhysics.DistanceMeasurement, 12), false)
            ).WithRemoveProhibited();
        }
    }
}