using NUnit.Framework;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Receptor.Vision
{
    [TestFixture]
    public class VisionReceptorTraitTest : ItemComponentTraitTestBase<ActorReference, SensoryReceptorData<VisionSense, VisionSense>,
        VisionSenseTrait<ActorReference>>
    {
        readonly VisionSenseReceptorPhysicsConfiguration physics;

        public VisionReceptorTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new VisionSenseReceptorPhysicsConfiguration(new LightPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev)));
        }

        protected override void SetUpPrepare()
        {
            EntityRegistry.RegisterNonConstructable<LightSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<VisionSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<VisionSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<VisionSense>>();

            EntityRegistry.RegisterNonConstructable<SensoryReceptorData<VisionSense, VisionSense>>();
            EntityRegistry.RegisterNonConstructable<SensoryReceptorState<VisionSense, VisionSense>>();
            EntityRegistry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<VisionSense, VisionSense>>();
            EntityRegistry.RegisterFlag<SenseReceptorDirtyFlag<VisionSense, VisionSense>>();
        }

        protected override VisionSenseTrait<ActorReference> CreateTrait()
        {
            return new VisionSenseTrait<ActorReference>(physics, 1.9f);
        }

        protected override IItemComponentTestDataFactory<SensoryReceptorData<VisionSense, VisionSense>> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<SensoryReceptorData<VisionSense, VisionSense>>(
                new SensoryReceptorData<VisionSense, VisionSense>(new SenseSourceDefinition(physics.VisionPhysics.DistanceMeasurement,
                                                                                            physics.VisionPhysics.AdjacencyRule,
                                                                                            1.9f), true),
                new SensoryReceptorData<VisionSense, VisionSense>(new SenseSourceDefinition(physics.VisionPhysics.DistanceMeasurement,
                                                                                            physics.VisionPhysics.AdjacencyRule,
                                                                                            10), true),
                new SensoryReceptorData<VisionSense, VisionSense>(new SenseSourceDefinition(physics.VisionPhysics.DistanceMeasurement,
                                                                                            physics.VisionPhysics.AdjacencyRule,
                                                                                            12), false)
            ).WithRemoveProhibited();
        }
    }
}