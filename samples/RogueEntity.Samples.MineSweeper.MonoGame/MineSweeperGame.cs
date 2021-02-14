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
using RogueEntity.Simple.MineSweeper;
using Serilog;
using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Console = SadConsole.Console;

namespace RogueEntity.Samples.MineSweeper.MonoGame
{
    public class MineSweeperGame
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        readonly DirectoryCatalog pluginCatalogue;

        IGameLoop gameLoop;
        MapConsole mapConsole;
        Optional<MineSweeperPlayerData> playerData;

        public IPlayerService<ActorReference> PlayerService { get; private set; }
        public IPlayerManager<ActorReference, MineSweeperPlayerProfile> PlayerManager { get; private set; }
        public IPlayerProfileManager<MineSweeperPlayerProfile> ProfileManager { get; private set; }
        public IServiceResolver ServiceResolver { get; private set; }
        public bool Started { get; private set; }

        public MineSweeperGame()
        {
            pluginCatalogue = new DirectoryCatalog(".");
        }

        public void InitializeSystems()
        {
            Log.Debug("Starting");
            ServiceResolver = new DefaultServiceResolver();
            ServiceResolver.Store<IEntityRandomGeneratorSource>(new DefaultRandomGeneratorSource(10, ServiceResolver.ResolveToReference<ITimeSource>()));

            var ms = new ModuleSystem(ServiceResolver);
            ms.ScanForModules("MineSweeper");

            gameLoop = ms.Initialize()
                         .BuildRealTimeStepLoop(30);
            
            ServiceResolver.Store(gameLoop.TimeSource);
            ServiceResolver.ValidatePromisesCanResolve();

            PlayerService = ServiceResolver.Resolve<IPlayerService<ActorReference>>();
            PlayerManager = ServiceResolver.Resolve<IPlayerManager<ActorReference, MineSweeperPlayerProfile>>();
            ProfileManager = ServiceResolver.Resolve<IPlayerProfileManager<MineSweeperPlayerProfile>>();
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
            if (Started)
            {
                return false;
            }
            
            gameLoop.Initialize();
            if (PlayerManager.TryActivatePlayer(playerProfileId, out var playerTag, out var playerEntityId, out var playerRecord))
            {
                Started = true;
                playerData = new MineSweeperPlayerData(playerTag, playerEntityId, playerRecord);
                return true;
            }

            return false;
        }

        public void Stop()
        {
            gameLoop.Dispose();
            Started = false;
        }

        public void Update(GameTime time)
        {
            if (!Started)
            {
                return;
            }

            gameLoop.Update(time.ElapsedGameTime);

            RefreshPlayerProfile();
        }

        void RefreshPlayerProfile()
        {
            if (playerData.TryGetValue(out var pd))
            {
                var ir = ServiceResolver.Resolve<IItemResolver<ActorReference>>();
                if (ir.TryQueryData(pd.PlayerEntityId, out MineSweeperPlayerProfile profile))
                {
                    pd.PlayerRecord = profile;
                }
            }
        }
    }
}
