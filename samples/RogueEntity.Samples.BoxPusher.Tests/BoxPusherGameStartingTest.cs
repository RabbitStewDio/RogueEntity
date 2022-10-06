using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Runtime;
using RogueEntity.Core.Storage;
using RogueEntity.Generator;
using RogueEntity.Samples.BoxPusher.Core;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using System;

namespace RogueEntity.Samples.BoxPusher.Tests
{
    [TestFixture]
    public class BoxPusherGameStartingTest
    {
        BoxPusherGame game;

        [SetUp]
        public void SetUp()
        {
            game = new BoxPusherGame(DefaultStorageLocationService.CreateDefault("BoxPusherTest"));
            game.InitializeSystems();
        }

        [Test]
        public void MapSystemInitialized()
        {
            Assert.NotNull(game.ServiceResolver);
            
            game.ServiceResolver.TryResolve<IMapRegionMetaDataService<int>>(out var mds).Should().BeTrue();
            game.ServiceResolver.TryResolve<IMapRegionEvictionSystem>(out _).Should().BeTrue();
            var x = (StaticMapLevelDataSource)mds;
            x!.Initialized.Should().BeTrue();
        }
        
        [Test]
        public void GameStarting()
        {
            
            game.ProfileManager.TryCreatePlayer(new BoxPusherPlayerProfile("UnitTest"), out var profileId, out var profile).Should().BeTrue();
            Assert.NotNull(profile);
            
            profile.PlayerName.Should().Be("UnitTest");
            profile.IsComplete(0).Should().BeFalse();
            profile.CurrentLevel.Should().Be(0);
            
            game.StartGame(profileId).Should().BeTrue();
            game.Update(TimeSpan.FromMilliseconds(200));
            game.Status.Should().NotBe(GameStatus.Error);
        }
        
        [Test]
        public void GameStartingWrongId()
        {
            game.StartGame(Guid.NewGuid()).Should().BeFalse();
        }
    }
}
