using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.Core.Runtime;
using RogueEntity.Core.Storage;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Samples.BoxPusher.Core
{
    public class BoxPusherGame : SinglePlayerGameBase<ActorReference>
    {
        readonly IStorageLocationService storageLocations;

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        readonly DirectoryCatalog pluginCatalogue;

        public IPlayerProfileManager<BoxPusherPlayerProfile> ProfileManager { get; private set; }
        public IMapRegionTrackerService<int> LevelLoader { get; private set; }
        public IItemResolver<ActorReference> ActorResolver { get; private set; }
        public BoxPusherPlayerStatusService StatusService { get; private set; }
        
        public BoxPusherGame(IStorageLocationService storageLocations) : base("BoxPusher")
        {
            this.storageLocations = storageLocations;
            pluginCatalogue = new DirectoryCatalog(".");
            GameInitialized += OnGameInitialized;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            ProfileManager = ServiceResolver.Resolve<IPlayerProfileManager<BoxPusherPlayerProfile>>();
            LevelLoader = ServiceResolver.Resolve<IMapRegionTrackerService<int>>();
            ActorResolver = ServiceResolver.Resolve<IItemResolver<ActorReference>>();
            StatusService = new BoxPusherPlayerStatusService(this, ActorResolver);
        }

        public Optional<BoxPusherPlayerProfile> CurrentPlayerProfile
        {
            get
            {
                if (PlayerData.TryGetValue(out var pd) && ActorResolver.TryQueryData(pd.EntityId, out BoxPusherPlayerProfile profileData))
                {
                    return profileData;
                }

                return default;
            }
        }

        public override bool IsBlockedOrWaitingForInput()
        {
            if (LevelLoader.IsBlocked())
            {
                Console.WriteLine("Blocked");
                return true;
            }

            return base.IsBlockedOrWaitingForInput();
        }

        protected override GameStatus CheckStatus()
        {
            if (LevelLoader.IsError())
            {
                Console.WriteLine("Level Loading failed");
                return GameStatus.Error;
            }

            // our boxpusher game does not have a clear win/lose condition. 
            return GameStatus.Running;
        }

        protected override void InitializeServices(IServiceResolver serviceResolver)
        {
            serviceResolver.Store(storageLocations);
            serviceResolver.Store<IEntityRandomGeneratorSource>(new DefaultRandomGeneratorSource(10, ServiceResolver.ResolveToReference<ITimeSource>()));
        }

        public bool StartGame(Guid profileId)
        {
            if (!ProfileManager.TryLoadPlayerData(profileId, out var profile))
            {
                return false;
            }

            if (StartGameWithPlayer(profileId) && this.PlayerData.TryGetValue(out var value))
            {
                if (ActorResolver.TryUpdateData(value.EntityId, profile, out _))
                {
                    Console.WriteLine("Player created");
                    return true;
                }

                ActorResolver.DiscardUnusedItem(value.EntityId);
            }

            return false;
        }
    }
}
