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
    public class SmellSourceTraitTest: ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, SmellSource, SmellSourceTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly SmellPhysicsConfiguration physics;

        public SmellSourceTraitTest(): base(new ActorReferenceMetaData())
        {
            physics = new SmellPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev));
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            EntityRegistry.RegisterNonConstructable<SmellSourceDefinition>();
            EntityRegistry.RegisterNonConstructable<SenseSourceState<SmellSense>>();
            EntityRegistry.RegisterFlag<ObservedSenseSource<SmellSense>>();
            EntityRegistry.RegisterFlag<SenseDirtyFlag<SmellSense>>();
            return context;
        }

        protected override SmellSourceTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new SmellSourceTrait<SenseMappingTestContext, ActorReference>(physics);
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