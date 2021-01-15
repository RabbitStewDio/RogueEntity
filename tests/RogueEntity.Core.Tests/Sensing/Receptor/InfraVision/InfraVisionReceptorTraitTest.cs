using NUnit.Framework;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.InfraVision
{
    [TestFixture]
    public class InfraVisionReceptorTraitTest : ItemComponentTraitTestBase<ActorReference, SensoryReceptorData<VisionSense, TemperatureSense>,
        InfraVisionSenseTrait<ActorReference>>
    {
        readonly HeatPhysicsConfiguration physics;

        public InfraVisionReceptorTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new HeatPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev), Temperature.FromCelsius(0));
        }

        protected override void SetUpPrepare()
        {
            EntityRegistry.RegisterNonConstructable<HeatSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<TemperatureSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<TemperatureSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<TemperatureSense>>();

            EntityRegistry.RegisterNonConstructable<SensoryReceptorData<VisionSense, TemperatureSense>>();
            EntityRegistry.RegisterNonConstructable<SensoryReceptorState<VisionSense, TemperatureSense>>();
            EntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>>();
            EntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<VisionSense, TemperatureSense>>();
        }

        protected override InfraVisionSenseTrait<ActorReference> CreateTrait()
        {
            var phy = new InfraVisionSenseReceptorPhysicsConfiguration(physics);
            return new InfraVisionSenseTrait<ActorReference>(phy, 1.9f);
        }

        protected override IItemComponentTestDataFactory<SensoryReceptorData<VisionSense, TemperatureSense>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<SensoryReceptorData<VisionSense, TemperatureSense>>(
                new SensoryReceptorData<VisionSense, TemperatureSense>(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement,
                                                                                                 physics.HeatPhysics.AdjacencyRule,
                                                                                                 1.9f), true),
                new SensoryReceptorData<VisionSense, TemperatureSense>(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement, 
                                                                                                 physics.HeatPhysics.AdjacencyRule,
                                                                                                 10), true),
                new SensoryReceptorData<VisionSense, TemperatureSense>(new SenseSourceDefinition(physics.HeatPhysics.DistanceMeasurement, 
                                                                                                 physics.HeatPhysics.AdjacencyRule,
                                                                                                 12), false)
            ).WithRemoveProhibited();
        }
    }
}