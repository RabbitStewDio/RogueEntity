using NUnit.Framework;
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
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Heat
{
    [TestFixture]
    public class HeatReceptorTraitTest : ItemComponentTraitTestBase<ActorReference, SensoryReceptorData<TemperatureSense, TemperatureSense>, HeatDirectionSenseTrait<ActorReference>>
    {
        readonly HeatSenseReceptorPhysicsConfiguration physics;

        public HeatReceptorTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new HeatSenseReceptorPhysicsConfiguration(
                new HeatPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev), Temperature.FromCelsius(0)),
                new FloodFillWorkingDataSource()
            );
        }

        protected override void SetUpPrepare()
        {
            EntityRegistry.RegisterNonConstructable<HeatSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<TemperatureSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<TemperatureSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<TemperatureSense>>();

            EntityRegistry.RegisterNonConstructable<SensoryReceptorData<TemperatureSense, TemperatureSense>>();
            EntityRegistry.RegisterNonConstructable<SensoryReceptorState<TemperatureSense, TemperatureSense>>();
            EntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TemperatureSense, TemperatureSense>>();
            EntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<TemperatureSense, TemperatureSense>>();
        }

        protected override HeatDirectionSenseTrait<ActorReference> CreateTrait()
        {
            return new HeatDirectionSenseTrait<ActorReference>(physics, 1.9f);
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