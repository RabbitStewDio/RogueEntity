using EnTTSharp.Annotations;
using NUnit.Framework;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Noise;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Sensing.Sources.Noise
{
    public class NoiseSourceTraitTest: ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, NoiseClip, NoiseSourceTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly NoisePhysicsConfiguration physics;

        public NoiseSourceTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new NoisePhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev));
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            EntityRegistry.RegisterNonConstructable<NoiseSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<NoiseSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<NoiseSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<NoiseSense>>();
            return context;
        }

        protected override NoiseSourceTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new NoiseSourceTrait<SenseMappingTestContext, ActorReference>(physics);
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