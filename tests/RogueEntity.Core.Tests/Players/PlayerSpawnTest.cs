using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Runtime;

namespace RogueEntity.Core.Tests.Players
{
    /// <summary>
    ///   Validates that players spawn at predefined spawn points and that
    ///   removing a player removes the entity and all remnants from the map
    ///   as well.
    /// </summary>
    [TestFixture]
    public class PlayerSpawnTest: BasicGameIntegrationTestBase
    {
        protected override void PrepareMapService(StaticTestMapService mapService)
        {
            base.PrepareMapService(mapService);
            mapService.AddMap(0, EmptyCorridor);
        }
        
        [Test]
        public void InitialStatusIsInitialized()
        {
            GameFixture.Status.Should().Be(GameStatus.Initialized);
        }
        
        [Test]
        public void AddPlayerToRun()
        {
            GameFixture.StartGame();
            GameFixture.Status.Should().Be(GameStatus.Running);
            GameFixture.PlayerData.HasValue.Should().BeTrue();
            GameFixture.PlayerService.TryQueryPrimaryObserver(GameFixture.PlayerData.Value.Tag, out _).Should().BeFalse("because player observers are only collected during the next update");
            
            GameFixture.Update(GameFixture.Time.FixedTimeStep);
            
            GameFixture.PlayerService.TryQueryPrimaryObserver(GameFixture.PlayerData.Value.Tag, out var obs).Should().BeTrue("because the default player is its own observer when on map");
            
            obs.Player.Should().Be(GameFixture.PlayerData.Value.Tag);
            obs.Primary.Should().BeFalse("Because primary designations must be managed explicitly.");
        }

        [Test]
        public void RemoveExistingPlayer()
        {
            GameFixture.StartGame();
            GameFixture.Update(GameFixture.Time.CurrentTime + GameFixture.Time.FixedTimeStep);
            GameFixture.PlayerService.TryQueryPrimaryObserver(GameFixture.PlayerData.Value.Tag, out _).Should().BeTrue("because the default player is its own observer when on map");
            GameFixture.Update(GameFixture.Time.CurrentTime + GameFixture.Time.FixedTimeStep);
            GameFixture.PlayerManager.TryDeactivatePlayer(GameFixture.PlayerData.Value.Tag.Id).Should().BeTrue();
            GameFixture.Update(GameFixture.Time.CurrentTime + GameFixture.Time.FixedTimeStep);
            GameFixture.PlayerData.HasValue.Should().BeFalse();
        }

        [Test]
        public void ReactivateExistingPlayer()
        {
            GameFixture.StartGame();
            GameFixture.Update(GameFixture.Time.CurrentTime + GameFixture.Time.FixedTimeStep);
            GameFixture.PlayerService.TryQueryPrimaryObserver(GameFixture.PlayerData.Value.Tag, out _).Should().BeTrue("because the default player is its own observer when on map");
            GameFixture.Update(GameFixture.Time.CurrentTime + GameFixture.Time.FixedTimeStep);
            GameFixture.PlayerManager.TryDeactivatePlayer(GameFixture.PlayerData.Value.Tag.Id).Should().BeTrue();
            GameFixture.Update(GameFixture.Time.CurrentTime + GameFixture.Time.FixedTimeStep);
            GameFixture.PlayerData.HasValue.Should().BeFalse();
            
            GameFixture.ReActivatePlayer();
            
            GameFixture.Update(GameFixture.Time.CurrentTime + GameFixture.Time.FixedTimeStep);
            GameFixture.PlayerService.TryQueryPrimaryObserver(GameFixture.PlayerData.Value.Tag, out _).Should().BeTrue("because the default player is its own observer when on map");


        }
    }
}
