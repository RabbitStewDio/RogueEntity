using EnTTSharp.Annotations;
using NUnit.Framework;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Smell;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Sources.Smell
{
    [TestFixture]
    public class SmellSourceTraitTest: ItemComponentTraitTestBase<ActorReference, SmellSource, SmellSourceTrait<ActorReference>>
    {
        readonly SmellPhysicsConfiguration physics;

        public SmellSourceTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new SmellPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev));
        }

        protected override void SetUpPrepare()
        {
            EntityRegistry.RegisterNonConstructable<SmellSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<SmellSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<SmellSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<SmellSense>>();
        }

        protected override SmellSourceTrait<ActorReference> CreateTrait()
        {
            return new SmellSourceTrait<ActorReference>(physics);
        }

        protected override IItemComponentTestDataFactory<SmellSource> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<SmellSource>(default,
                                                                 new SmellSource(10, "Tag1"),
                                                                 new SmellSource(15, "Tag2")
            );
        }

        protected override EntityComponentRegistration PerformEntityComponentRegistration(EntityRegistrationScanner scn)
        {
            if (!scn.TryRegisterComponent<SmellSourceDefinition>(out var registration))
            {
                Assert.Fail("Unable to register component type.");
            }

            return registration;
        }
    }
}