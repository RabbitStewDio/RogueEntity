using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Smell;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Smell;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Smell
{
    public class SmellReceptorTraitTest : ItemComponentTraitTestBase<ActorReference, SensoryReceptorData<SmellSense, SmellSense>,
        SmellDirectionSenseTrait<ActorReference>>
    {
        readonly SmellSenseReceptorPhysicsConfiguration physics;

        public SmellReceptorTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new SmellSenseReceptorPhysicsConfiguration(
                new SmellPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev)),
                new FloodFillWorkingDataSource()
            );
        }

        protected override void SetUpPrepare()
        {
            EntityRegistry.RegisterNonConstructable<SmellSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<SmellSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<SmellSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<SmellSense>>();

            EntityRegistry.RegisterNonConstructable<SensoryReceptorData<SmellSense, SmellSense>>();
            EntityRegistry.RegisterNonConstructable<SensoryReceptorState<SmellSense, SmellSense>>();
            EntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<SmellSense, SmellSense>>();
            EntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<SmellSense, SmellSense>>();
        }

        protected override SmellDirectionSenseTrait<ActorReference> CreateTrait()
        {
            return new SmellDirectionSenseTrait<ActorReference>(physics, 1.9f);
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