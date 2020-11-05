using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Noise;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Noise;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Noise
{
    public class NoiseReceptorTraitTest : ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, SensoryReceptorData<NoiseSense, NoiseSense>,
        NoiseDirectionSenseTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly NoiseSenseReceptorPhysicsConfiguration physics;

        public NoiseReceptorTraitTest()
        {
            physics = new NoiseSenseReceptorPhysicsConfiguration(
                new NoisePhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev)),
                new FloodFillWorkingDataSource()
            );
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<NoiseSourceDefinition>();
            context.ActorEntityRegistry.RegisterNonConstructable<SenseSourceState<NoiseSense>>();
            context.ActorEntityRegistry.RegisterFlag<ObservedSenseSource<NoiseSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseDirtyFlag<NoiseSense>>();

            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorData<NoiseSense, NoiseSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorState<NoiseSense, NoiseSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<NoiseSense, NoiseSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<NoiseSense, NoiseSense>>();

            return context;
        }

        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override ItemRegistry<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override NoiseDirectionSenseTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new NoiseDirectionSenseTrait<SenseMappingTestContext, ActorReference>(physics, 1.9f);
        }

        protected override IItemComponentTestDataFactory<SensoryReceptorData<NoiseSense, NoiseSense>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<SensoryReceptorData<NoiseSense, NoiseSense>>(
                new SensoryReceptorData<NoiseSense, NoiseSense>(new SenseSourceDefinition(physics.NoisePhysics.DistanceMeasurement,
                                                                                          physics.NoisePhysics.AdjacencyRule,
                                                                                          1.9f), true),
                new SensoryReceptorData<NoiseSense, NoiseSense>(new SenseSourceDefinition(physics.NoisePhysics.DistanceMeasurement, 
                                                                                          physics.NoisePhysics.AdjacencyRule,
                                                                                          10), true),
                new SensoryReceptorData<NoiseSense, NoiseSense>(new SenseSourceDefinition(physics.NoisePhysics.DistanceMeasurement, 
                                                                                          physics.NoisePhysics.AdjacencyRule,
                                                                                          12), false)
            ).WithRemoveProhibited();
        }
    }
}