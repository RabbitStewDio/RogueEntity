using Microsoft.Xna.Framework;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.SadCons;
using RogueEntity.Simple.BoxPusher.ItemTraits;
using Serilog;
using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Console = SadConsole.Console;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherGame
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        readonly DirectoryCatalog pluginCatalogue;

        IGameLoop gameLoop;
        MapConsole mapConsole;
        Optional<PlayerData> playerData;

        public IPlayerService<ActorReference> PlayerService { get; private set; }
        public IPlayerManager<ActorReference, BoxPusherPlayerProfile> PlayerManager { get; private set; }
        public IPlayerProfileManager<BoxPusherPlayerProfile> ProfileManager { get; private set; }
        public IServiceResolver ServiceResolver { get; private set; }

        public BoxPusherGame()
        {
            pluginCatalogue = new DirectoryCatalog(".");
        }

        public void InitializeSystems()
        {
            Log.Debug("Starting");
            ServiceResolver = new DefaultServiceResolver();
            ServiceResolver.Store<IEntityRandomGeneratorSource>(new DefaultRandomGeneratorSource(10, ServiceResolver.ResolveToReference<ITimeSource>()));

            var ms = new ModuleSystem(ServiceResolver);
            ms.ScanForModules("BoxPusher");

            gameLoop = ms.Initialize().BuildRealTimeStepLoop(30);
            ServiceResolver.Store(gameLoop.TimeSource);
            ServiceResolver.ValidatePromisesCanResolve();

            PlayerService = ServiceResolver.Resolve<IPlayerService<ActorReference>>();
            PlayerManager = ServiceResolver.Resolve<IPlayerManager<ActorReference, BoxPusherPlayerProfile>>();
            ProfileManager = ServiceResolver.Resolve<IPlayerProfileManager<BoxPusherPlayerProfile>>();
        }
        
        public Console InitializeRendering(int width, int height)
        {
            var console = new Console(width, height);
            console.FillWithRandomGarbage();
            
            mapConsole = new MapConsole(console);
            mapConsole.Initialize(PlayerService);
            return console;
        }

        /// <summary>
        ///    Starts a new game, either using the existing player data identified by the given Guid
        ///    or a new, randomly generated player id.
        /// </summary>
        /// <param name="playerProfileId"></param>
        public bool StartGame(Guid playerProfileId)
        {
            if (PlayerManager.TryActivatePlayer(playerProfileId, out var playerTag, out var playerEntityId, out var playerRecord))
            {
                gameLoop.Initialize();
                
                playerData = new PlayerData(playerTag, playerEntityId, playerRecord);
                return true;
            }

            return false;
        }

        public void Stop()
        {
            gameLoop.Dispose();
        }
        
        public void Update(GameTime time)
        {
            gameLoop.Update(time.ElapsedGameTime);
            
            if (playerData.TryGetValue(out var pd))
            {
                var ir = ServiceResolver.Resolve<IItemResolver<ActorReference>>();
                if (ir.TryQueryData(pd.PlayerEntityId, out BoxPusherPlayerProfile profile))
                {
                    pd.PlayerRecord = profile;
                }
            }
        }


    }
    
    public class PlayerData
    {
        public BoxPusherPlayerProfile PlayerRecord { get; set; }
        public readonly PlayerTag PlayerTag;
        public readonly ActorReference PlayerEntityId;

        public PlayerData(in PlayerTag playerTag, in ActorReference playerEntityId, BoxPusherPlayerProfile playerRecord)
        {
            PlayerRecord = playerRecord;
            this.PlayerTag = playerTag;
            this.PlayerEntityId = playerEntityId;
        }
    }

}