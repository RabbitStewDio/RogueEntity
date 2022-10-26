using System;
using EnTTSharp.Annotations;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Sensing.Discovery;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Sensing.Discovery
{
    [TestFixture]
    public class DiscoveryMapTraitTest: ItemComponentInformationTraitTestBase<ActorReference, IDiscoveryMap, DiscoveryMapTrait<ActorReference>>
    {
        public DiscoveryMapTraitTest() : base(new ActorReferenceMetaData())
        {
        }

        protected override void SetUpPrepare()
        {
            EntityRegistry.RegisterNonConstructable<DiscoveryMapData>();
        }

        protected override IItemComponentTestDataFactory<IDiscoveryMap> ProduceTestData(EntityRelations<ActorReference> relations)
        {
            if (!ItemResolver.TryQueryData(relations.DefaultEntityId, out IDiscoveryMap d))
            {
                throw new ArgumentException();
            }
            var writable = (DiscoveryMapData)d;
            return new ItemComponentTestDataFactory<IDiscoveryMap>(ProduceDiscoveryData(writable));
        }

        protected override DiscoveryMapTrait<ActorReference> CreateTrait()
        {
            return new DiscoveryMapTrait<ActorReference>();
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
            if (data.TryGetWritableView(0, out var mapData))
            {
                mapData.TrySet(10, 10, true).Should().BeTrue();
                mapData.TrySet(11, 10, true).Should().BeTrue();
            }

            return data;
        }
    }
}