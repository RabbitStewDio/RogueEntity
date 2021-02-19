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
using RogueEntity.Core.Utils;
using RogueEntity.Samples.MineSweeper.Core.Commands;
using RogueEntity.Samples.MineSweeper.Core.Services;
using RogueEntity.Samples.MineSweeper.Core.Traits;
using Serilog;
using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Samples.MineSweeper.MonoGame
{
    [Flags]
    public enum MineSweeperGameStatus
    {
        NotStarted = 0b0000,
        Initialized = 0b0001,
        Running = 0b0011,
        GameLost = 0b1001,
        GameWon = 0b0101,
    }

    public static class MineSweeperGameStatusExtensions
    {
        public static bool IsInitialized(this MineSweeperGameStatus s) => s.HasFlags(MineSweeperGameStatus.Initialized);

        public static bool IsStoppable(this MineSweeperGameStatus s) => s.HasFlags(MineSweeperGameStatus.Running) ||
                                                                        s.HasFlags(MineSweeperGameStatus.GameLost) ||
                                                                        s.HasFlags(MineSweeperGameStatus.GameWon);
    }

    public class MineSweeperGame
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        readonly DirectoryCatalog pluginCatalogue;

        IGameLoop gameLoop;
        Optional<PlayerReference> playerData;
        DefaultRandomGeneratorSource randomGeneratorSource;

        public MineSweeperGameParameterService GameParameterService { get; }
        public IPlayerService<ActorReference> PlayerService { get; private set; }
        public IPlayerManager<ActorReference> PlayerManager { get; private set; }
        public MineSweeperCommandService<ActorReference> CommandService { get; private set; }

        public IServiceResolver ServiceResolver { get; private set; }
        public MineSweeperGameStatus Status { get; private set; }

        public event EventHandler GameInitialized;
        public event EventHandler GameStarted;
        public event EventHandler GameFinished;
        public event EventHandler GameStopped;

        public MineSweeperGame()
        {
            pluginCatalogue = new DirectoryCatalog(".");
            GameParameterService = new MineSweeperGameParameterService();
        }

        public Optional<PlayerReference> PlayerData => playerData;

        public void InitializeSystems()
        {
            Log.Debug("Starting");
            ServiceResolver = new DefaultServiceResolver();

            randomGeneratorSource = new DefaultRandomGeneratorSource(10, ServiceResolver.ResolveToReference<ITimeSource>());
            ServiceResolver.Store<IEntityRandomGeneratorSource>(randomGeneratorSource);
            ServiceResolver.Store<IMineSweeperGameParameterService>(GameParameterService);

            var ms = new ModuleSystem(ServiceResolver);
            ms.ScanForModules("MineSweeper");

            gameLoop = ms.Initialize()
                         .BuildRealTimeStepLoop(30);

            ServiceResolver.Store(gameLoop.TimeSource);
            ServiceResolver.ValidatePromisesCanResolve();

            PlayerService = ServiceResolver.Resolve<IPlayerService<ActorReference>>();
            PlayerManager = ServiceResolver.Resolve<IPlayerManager<ActorReference>>();
            CommandService = ServiceResolver.Resolve<MineSweeperCommandService<ActorReference>>();

            Status = MineSweeperGameStatus.Initialized;
            GameInitialized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///    Starts a new game, either using the existing player data identified by the given Guid
        ///    or a new, randomly generated player id.
        /// </summary>
        public bool StartGame(MineSweeperGameParameter param)
        {
            if (Status == MineSweeperGameStatus.Running)
            {
                return false;
            }

            GameParameterService.WorldParameter = param;
            randomGeneratorSource.Seed = param.Seed;
            gameLoop.Initialize(() => CheckGameOver() == MineSweeperGameStatus.Running);

            if (PlayerManager.TryActivatePlayer(PlayerIds.SinglePlayer, out var playerTag, out var playerEntityId))
            {
                playerData = new PlayerReference(playerTag, playerEntityId);

                Status = MineSweeperGameStatus.Running;
                GameStarted?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }


        MineSweeperGameStatus CheckGameOver()
        {
            if (!PlayerData.TryGetValue(out var pd))
            {
                return MineSweeperGameStatus.Initialized;
            }

            var actorResolver = ServiceResolver.Resolve<IItemResolver<ActorReference>>();
            if (actorResolver.TryQueryData(pd.EntityId, out MineSweeperPlayerData data))
            {
                if (data.ExplodedPosition.HasValue)
                {
                    return MineSweeperGameStatus.GameLost;
                }

                if (data.AreaCleared)
                {
                    return MineSweeperGameStatus.GameWon;
                }
            }

            return MineSweeperGameStatus.Running;
        }

        public void Stop()
        {
            if (!Status.IsStoppable())
            {
                return;
            }

            gameLoop.Stop();
            Status = MineSweeperGameStatus.Initialized;
            GameStopped?.Invoke(this, EventArgs.Empty);
        }

        public void Update(GameTime time)
        {
            if (Status != MineSweeperGameStatus.Running)
            {
                return;
            }

            gameLoop.Update(time.ElapsedGameTime);

            var status = CheckGameOver();
            if (status == MineSweeperGameStatus.GameLost ||
                status == MineSweeperGameStatus.GameWon)
            {
                Status = status;
                GameFinished?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
