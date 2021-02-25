using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.MapLoading;
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
    public class BoxPusherGame: GameBase<ActorReference>
    {
        readonly IStorageLocationService storageLocations;

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        readonly DirectoryCatalog pluginCatalogue;

        public IPlayerProfileManager<BoxPusherPlayerProfile> ProfileManager { get; private set; }
        public IMapRegionLoaderService<int> LevelLoader { get; private set; }
        IItemResolver<ActorReference> actorResolver;

        public BoxPusherGame(IStorageLocationService storageLocations): base("BoxPusher")
        {
            this.storageLocations = storageLocations;
            pluginCatalogue = new DirectoryCatalog(".");
            GameInitialized += OnGameInitialized;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            ProfileManager = ServiceResolver.Resolve<IPlayerProfileManager<BoxPusherPlayerProfile>>();
            LevelLoader = ServiceResolver.Resolve<IMapRegionLoaderService<int>>();
            actorResolver = ServiceResolver.Resolve<IItemResolver<ActorReference>>();
        }

        public override bool IsBlockedOrWaitingForInput()
        {
            if (LevelLoader.IsBlocked())
            {
                System.Console.WriteLine("Blocked");
                return true;
            }
            
            return base.IsBlockedOrWaitingForInput();
        }
        
        protected override GameStatus CheckStatus()
        {
            if (LevelLoader.IsError())
            {
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

            if (PerformStartGame(profileId) && this.PlayerData.TryGetValue(out var value))
            {
                if (actorResolver.TryUpdateData(value.EntityId, profile, out _) &&
                    actorResolver.TryUpdateData(value.EntityId, PlayerObserverTag.CreateFor(new PlayerTag(profileId)), out _))
                {
                    System.Console.WriteLine("Player created");
                    return true;
                }

                actorResolver.DiscardUnusedItem(value.EntityId);
            }

            return false;
        }
    }
}