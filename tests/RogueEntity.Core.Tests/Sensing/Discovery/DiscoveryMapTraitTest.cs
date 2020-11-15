using System;
using EnTTSharp.Annotations;
using NUnit.Framework;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Sensing.Discovery
{
    [TestFixture]
    public class DiscoveryMapTraitTest: ItemComponentInformationTraitTestBase<SenseMappingTestContext, ActorReference, IDiscoveryMap, DiscoveryMapTrait<SenseMappingTestContext, ActorReference>>
    {
        public DiscoveryMapTraitTest() : base(new ActorReferenceMetaData())
        {
        }

        protected override SenseMappingTestContext CreateContext()
        {
            var context = new SenseMappingTestContext();
            EntityRegistry.RegisterNonConstructable<DiscoveryMapData>();

            return context;
        }

        protected override IItemComponentTestDataFactory<IDiscoveryMap> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            if (!ItemResolver.TryQueryData(relations.DefaultEntityId, Context, out IDiscoveryMap d))
            {
                throw new ArgumentException();
            }
            var writable = (DiscoveryMapData)d;
            return new ItemComponentTestDataFactory<IDiscoveryMap>(ProduceDiscoveryData(writable));
        }

        protected override DiscoveryMapTrait<SenseMappingTestContext, ActorReference> CreateTrait()
        {
            return new DiscoveryMapTrait<SenseMappingTestContext, ActorReference>();
        }

        protected override EntityComponentRegistration PerformEntityComponentRegistration(EntityRegistrationScanner scn)
        {
            if (!scn.TryRegisterComponent<DiscoveryMapData>(out var registration))
            {
                Assert.Fail("Unable to register component type.");
            }

            return registration;
        }

        static DiscoveryMapData ProduceDiscoveryData(DiscoveryMapData data)
        {
            if (data.TryGetWritableMap(0, out var mapData))
            {
                mapData[10, 10] = true;
                mapData[11, 10] = true;
            }

            return data;
        }
    }
}