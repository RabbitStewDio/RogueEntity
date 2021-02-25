using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Players;
using Serilog;
using System;

namespace RogueEntity.Core.Runtime
{
    public abstract class GameBase<TPlayerEntity>
    {
        public event EventHandler GameInitialized;
        public event EventHandler GameStarted;
        public event EventHandler GameFinished;
        public event EventHandler GameStopped;

        string[] ModuleIds { get; }
        IGameLoop gameLoop;

        protected GameBase(params string[] moduleIds)
        {
            ModuleIds = moduleIds;
        }

        public IServiceResolver ServiceResolver { get; private set; }
        public IPlayerService PlayerService { get; private set; }
        public IPlayerManager<TPlayerEntity> PlayerManager { get; private set; }
        public GameStatus Status { get; private set; }
        public Optional<PlayerReference<TPlayerEntity>> PlayerData { get; private set; }

        protected virtual IServiceResolver CreateServiceResolver() => new DefaultServiceResolver();
        
        public void InitializeSystems()
        {
            Log.Debug("Starting");
            ServiceResolver = CreateServiceResolver();
            InitializeServices(ServiceResolver);
            
            var ms = new ModuleSystem(ServiceResolver);
            ms.ScanForModules(ModuleIds);

            gameLoop = ms.Initialize()
                         .BuildRealTimeStepLoop(30);

            ServiceResolver.Store(gameLoop.TimeSource);
            ServiceResolver.ValidatePromisesCanResolve();
            PlayerService = ServiceResolver.Resolve<IPlayerService>();
            PlayerManager = ServiceResolver.Resolve<IPlayerManager<TPlayerEntity>>();
            
            Status = GameStatus.Initialized;
            GameInitialized?.Invoke(this, EventArgs.Empty);
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

            var status = CheckStatus();
            if (status.IsFinished())
            {
                Status = status;
                GameFinished?.Invoke(this, EventArgs.Empty);
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return CheckStatus() != GameStatus.Running;
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
        }

        protected abstract GameStatus CheckStatus();
    }
}