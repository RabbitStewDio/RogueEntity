using EnTTSharp.Annotations;
using NUnit.Framework;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Noise;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Sources.Noise
{
    public class NoiseSourceTraitTest: ItemComponentTraitTestBase<ActorReference, NoiseClip, NoiseSourceTrait<ActorReference>>
    {
        readonly NoisePhysicsConfiguration physics;

        public NoiseSourceTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new NoisePhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev));
        }

        protected override void SetUpPrepare()
        {
            EntityRegistry.RegisterNonConstructable<NoiseSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<NoiseSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<NoiseSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<NoiseSense>>();
        }

        protected override NoiseSourceTrait<ActorReference> CreateTrait()
        {
            return new NoiseSourceTrait<ActorReference>(physics);
        }

        protected override IItemComponentTestDataFactory<NoiseClip> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<NoiseClip>(default,
                                                               new NoiseClip(10, "Tag1"),
                                                               new NoiseClip(15, "Tag2")
            );
        }

        protected override EntityComponentRegistration PerformEntityComponentRegistration(EntityRegistrationScanner scn)
        {
            if (!scn.TryRegisterComponent<NoiseSourceDefinition>(out var registration))
            {
                Assert.Fail("Unable to register component type.");
            }

            return registration;
        }
    }
}