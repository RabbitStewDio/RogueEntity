using EnTTSharp.Annotations;
using EnTTSharp.Entities;
using NUnit.Framework;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Smell;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Tests.Sensing.Sources.Smell
{
    [TestFixture]
    public class SmellSourceTraitTest: ItemComponentTraitTestBase<SenseMappingTestContext, ActorReference, SmellSource, SmellSourceTrait<SenseMappingTestContext, ActorReference>>
    {
        readonly SmellPhysicsConfiguration physics;

        public SmellSourceTraitTest()
        {
            physics = new SmellPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Chebyshev));
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<SmellSourceDefinition>();
            context.ActorEntityRegistry.RegisterNonConstructable<SenseSourceState<SmellSense>>();
            context.ActorEntityRegistry.RegisterFlag<ObservedSenseSource<SmellSense>>();
            context.ActorEntityRegistry.RegisterFlag<SenseDirtyFlag<SmellSense>>();
            return context;
        }

        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override ItemRegistry<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();

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