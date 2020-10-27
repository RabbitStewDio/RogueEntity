using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.InfraVision
{
    public class InfraVisionReceptorTraitTest: ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, SensoryReceptorData<VisionSense>, InfraVisionSenseTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly HeatPhysicsConfiguration physics;

        public InfraVisionReceptorTraitTest()
        {
            physics = new HeatPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev), Temperature.FromCelsius(0));
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<HeatSourceDefinition>();
            context.ActorEntityRegistry.RegisterNonConstructable<SenseSourceState<TemperatureSense>>();
            context.ActorEntityRegistry.RegisterFlag<ObservedSenseSource<TemperatureSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseDirtyFlag<TemperatureSense>>();
            
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorData<VisionSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SensoryReceptorState<VisionSense>>();
            context.ActorEntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<VisionSense>>();

            return context;
        }
        
        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override ItemRegistry<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

        protected override InfraVisionSenseTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            var phy = new InfraVisionSenseReceptorPhysicsConfiguration(physics);
            return new InfraVisionSenseTrait<SenseMappingTestContext, ActorReference>(phy, 1.9f);
        }

        protected override IItemComponentTestDataFactory<SensoryReceptorData<VisionSense>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<SensoryReceptorData<VisionSense>>(new SensoryReceptorData<VisionSense>(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement, 1.9f), true),
                                                                                      new SensoryReceptorData<VisionSense>(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement, 10), true),
                                                                                      new SensoryReceptorData<VisionSense>(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement, 12), false)
            ).WithRemoveProhibited();
        }
    }
}