using EnTTSharp.Annotations;
using EnTTSharp.Entities;
using NUnit.Framework;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Sensing
{
    [TestFixture]
    public class OnDemandDiscoveryMapTraitTest: ItemComponentInformationTraitTestBase<SenseMappingTestContext, ActorReference, IDiscoveryMapData, OnDemandDiscoveryMapTrait<SenseMappingTestContext, ActorReference>>
    {
        protected override EntityRegistry<ActorReference> EntityRegistry => Context.ActorEntityRegistry;
        protected override ItemRegistry<SenseMappingTestContext, ActorReference> ItemRegistry => Context.ActorRegistry;
        protected override IBulkDataStorageMetaData<ActorReference> ItemIdMetaData => new ActorReferenceMetaData();
        
        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            context.ActorEntityRegistry.RegisterNonConstructable<OnDemandDiscoveryMapData>();

            return context;
        }

        protected override IItemComponentTestDataFactory<IDiscoveryMapData> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            return new ItemComponentTestDataFactory<IDiscoveryMapData>(ProduceDiscoveryData());
        }

        protected override OnDemandDiscoveryMapTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new PopulatedDiscoveryMapTrait(100, 100);
        }

        protected override EntityComponentRegistration PerformEntityComponentRegistration(EntityRegistrationScanner scn)
        {
            if (!scn.TryRegisterComponent<OnDemandDiscoveryMapData>(out var registration))
            {
                Assert.Fail("Unable to register component type.");
            }

            return registration;
        }

        class PopulatedDiscoveryMapTrait : OnDemandDiscoveryMapTrait<SenseMappingTestContext, ActorReference>
        {
            public PopulatedDiscoveryMapTrait(int width, int height) : base(width, height)
            {
            }

            protected override OnDemandDiscoveryMapData CreateValue(SenseMappingTestContext context, ActorReference k, IItemDeclaration item)
            {
                return ProduceDiscoveryData();
            }
        }

        static OnDemandDiscoveryMapData ProduceDiscoveryData()
        {
            var data = new OnDemandDiscoveryMapData(100, 100);
            if (data.TryGetMap(0, out var mapData))
            {
                mapData[10, 10] = true;
                mapData[11, 10] = true;
            }

            return data;
        }
    }
}