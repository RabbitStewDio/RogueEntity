using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Tests.Fixtures;
using System;

namespace RogueEntity.Core.Tests.Meta
{
    [TestFixture]
    public class CoreModuleTest: BasicGameIntegrationTestBase
    {
        [Test]
        public void EnsureEntitiesResetOnStop()
        {
            GameFixture.StartGame();
            GameFixture.Update(TimeSpan.FromSeconds(0.5));
            var ir = GameFixture.ServiceResolver!.Resolve<IItemResolver<ActorReference>>();
            var ek = ir.Instantiate(StandardEntityDefinitions.Player.Id);
            
            GameFixture.Stop();

            ir.IsDestroyed(ek).Should().BeTrue();
        }
    }
}
