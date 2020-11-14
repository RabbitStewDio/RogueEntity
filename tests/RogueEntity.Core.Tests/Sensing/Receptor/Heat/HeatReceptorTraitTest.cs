using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Heat;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Heat
{
    public class HeatReceptorTraitTest : ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, SensoryReceptorData<TemperatureSense, TemperatureSense>,
        HeatDirectionSenseTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly HeatSenseReceptorPhysicsConfiguration physics;

        public HeatReceptorTraitTest()
        {
            physics = new HeatSenseReceptorPhysicsConfiguration(
                new HeatPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev), Temperature.FromCelsius(0)),
                new FloodFillWorkingDataSource()
            );
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<HeatSourceDefinition>();
            context.ActorEntityRegistry.RegisterNonConstructable<SenseSourceState<TemperatureSense>>();
            context.ActorEntityRegistry.RegisterFlag<ObservedSenseSource<TemperatureSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseDirtyFlag<TemperatureSense>>();

            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorData<TemperatureSense, TemperatureSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorState<TemperatureSense, TemperatureSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TemperatureSense, TemperatureSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<TemperatureSense, TemperatureSense>>();

            return context;
        }

        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override IItemRegistryBackend<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override HeatDirectionSenseTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new HeatDirectionSenseTrait<SenseMappingTestContext, ActorReference>(physics, 1.9f);
        }

        protected override IItemComponentTestDataFactory<SensoryReceptorData<TemperatureSense, TemperatureSense>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<SensoryReceptorData<TemperatureSense, TemperatureSense>>(
                new SensoryReceptorData<TemperatureSense, TemperatureSense>(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement, 
                                                                                                      physics.HeatPhysics.AdjacencyRule, 1.9f), true),
                new SensoryReceptorData<TemperatureSense, TemperatureSense>(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement, 
                                                                                                      physics.HeatPhysics.AdjacencyRule,
                                                                                                      10), true),
                new SensoryReceptorData<TemperatureSense, TemperatureSense>(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement, 
                                                                                                      physics.HeatPhysics.AdjacencyRule,
                                                                                                      12), false)
            ).WithRemoveProhibited();
        }
    }
}