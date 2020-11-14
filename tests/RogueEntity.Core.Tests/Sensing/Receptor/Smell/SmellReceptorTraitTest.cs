using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Smell;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Smell;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Smell
{
    public class SmellReceptorTraitTest : ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, SensoryReceptorData<SmellSense, SmellSense>,
        SmellDirectionSenseTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly SmellSenseReceptorPhysicsConfiguration physics;

        public SmellReceptorTraitTest()
        {
            physics = new SmellSenseReceptorPhysicsConfiguration(
                new SmellPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev)),
                new FloodFillWorkingDataSource()
            );
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<SmellSourceDefinition>();
            context.ActorEntityRegistry.RegisterNonConstructable<SenseSourceState<SmellSense>>();
            context.ActorEntityRegistry.RegisterFlag<ObservedSenseSource<SmellSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseDirtyFlag<SmellSense>>();

            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorData<SmellSense, SmellSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorState<SmellSense, SmellSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<SmellSense, SmellSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<SmellSense, SmellSense>>();

            return context;
        }

        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override IItemRegistryBackend<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override SmellDirectionSenseTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new SmellDirectionSenseTrait<SenseMappingTestContext, ActorReference>(physics, 1.9f);
        }

        protected override IItemComponentTestDataFactory<SensoryReceptorData<SmellSense, SmellSense>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<SensoryReceptorData<SmellSense, SmellSense>>(
                new SensoryReceptorData<SmellSense, SmellSense>(new SenseSourceDefinition(physics.SmellPhysics.DistanceMeasurement, 
                                                                                          physics.SmellPhysics.AdjacencyRule,
                                                                                          1.9f), true),
                new SensoryReceptorData<SmellSense, SmellSense>(new SenseSourceDefinition(physics.SmellPhysics.DistanceMeasurement, 
                                                                                          physics.SmellPhysics.AdjacencyRule,
                                                                                          10), true),
                new SensoryReceptorData<SmellSense, SmellSense>(new SenseSourceDefinition(physics.SmellPhysics.DistanceMeasurement, 
                                                                                          physics.SmellPhysics.AdjacencyRule,
                                                                                          12), false)
            ).WithRemoveProhibited();
        }
    }
}