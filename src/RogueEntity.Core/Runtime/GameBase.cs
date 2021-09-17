using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using Serilog;
using System;

namespace RogueEntity.Core.Runtime
{
    public abstract class GameBase<TPlayerEntity>
    {
        static readonly ILogger Logger = SLog.ForContext<GameBase<TPlayerEntity>>();
        
        public event EventHandler GameInitialized;
        public event EventHandler GameStarted;
        public event EventHandler GameFinished;
        public event EventHandler GameStopped;
        public event EventHandler<TimeSpan> GameUpdate;

        string[] ModuleIds { get; }
        IGameLoop gameLoop;

        protected GameBase(params string[] moduleIds)
        {
            ModuleIds = moduleIds ?? new string[0];
        }

        public ITimeSourceDefinition TimeSourceDefinition { get; protected set; }
        public IServiceResolver ServiceResolver { get; private set; }
        public IPlayerService PlayerService { get; private set; }
        public IPlayerManager<TPlayerEntity> PlayerManager { get; private set; }
        public GameStatus Status { get; private set; }
        public Optional<PlayerReference<TPlayerEntity>> PlayerData { get; private set; }
        public IBasicCommandService<TPlayerEntity> CommandService { get; private set; }

        protected virtual IServiceResolver CreateServiceResolver() => new DefaultServiceResolver();
        
        public void InitializeSystems()
        {
            Logger.Debug("Starting");
            TimeSourceDefinition ??= new RealTimeSourceDefinition(30);
            
            ServiceResolver = CreateServiceResolver();
            ServiceResolver.Store(TimeSourceDefinition);
            InitializeServices(ServiceResolver);

            var ms = CreateModuleSystem();

            gameLoop = TimeSourceDefinition.BuildTimeStepLoop(ms.Initialize());

            ServiceResolver.Store(gameLoop.TimeSource);
            ServiceResolver.ValidatePromisesCanResolve();
            PlayerService = ServiceResolver.Resolve<IPlayerService>();
            PlayerManager = ServiceResolver.Resolve<IPlayerManager<TPlayerEntity>>();
            CommandService = ServiceResolver.Resolve<IBasicCommandService<TPlayerEntity>>();

            Status = GameStatus.Initialized;
            GameInitialized?.Invoke(this, EventArgs.Empty);
        }

        protected virtual ModuleSystem CreateModuleSystem()
        {
            var ms = new ModuleSystem(ServiceResolver);
            ms.ScanForModules(ModuleIds);
            return ms;
        }

        protected virtual void InitializeServices(IServiceResolver serviceResolver)
        {
        }

        protected bool PerformStartGame(Guid playerId = default)
        {
            if (Status != GameStatus.Initialized)
            {
                return false;
            }

            if (playerId == default)
            {
                playerId = PlayerIds.SinglePlayer;
            }
            
            gameLoop.Initialize(IsBlockedOrWaitingForInput);

            if (PlayerManager.TryActivatePlayer(playerId, out var playerTag, out var playerEntityId))
            {
                PlayerData = new PlayerReference<TPlayerEntity>(playerTag, playerEntityId);

                Status = GameStatus.Running;
                GameStarted?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }
        
        public void Update(TimeSpan absoluteGameTime)
        {
            if (Status != GameStatus.Running)
            {
                return;
            }

            gameLoop.Update(absoluteGameTime);
            UpdateOverride(absoluteGameTime);
            GameUpdate?.Invoke(this, absoluteGameTime);

            var status = CheckStatus();
            if (status.IsFinished())
            {
                Status = status;
                GameFinished?.Invoke(this, EventArgs.Empty);
                RemoveActivePlayer();
            }
        }

        void RemoveActivePlayer()
        {
            if (!PlayerData.TryGetValue(out var playerData) ||
                !PlayerManager.TryDeactivatePlayer(playerData.Tag.Id))
            {
                // Big warning, unable to kill player.
            }
        }

        public virtual bool IsBlockedOrWaitingForInput()
        {
            // if status is error, or either a win or a loose, 
            // we don't continue simulating the world. 
            switch (CheckStatus())
            {
                case GameStatus.NotStarted:
                case GameStatus.Initialized:
                case GameStatus.GameLost:
                case GameStatus.GameWon:
                case GameStatus.Error:
                    return true;
                case GameStatus.Running:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected virtual void UpdateOverride(TimeSpan absoluteGameTime)
        {
        }

        public virtual void Stop()
        {
            if (!Status.IsStoppable())
            {
                return;
            }

            gameLoop.Stop();
            Status = GameStatus.Initialized;
            GameStopped?.Invoke(this, EventArgs.Empty);
            RemoveActivePlayer();
        }

        protected virtual GameStatus CheckStatus() => GameStatus.Running;
    }
}
