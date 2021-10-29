using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Services;
using Serilog;
using System;

namespace RogueEntity.Core.Runtime
{
    public abstract class GameBase: IDisposable
    {
        static readonly ILogger Logger = SLog.ForContext<GameBase>();

        event EventHandler gameInitializedInternal;
        
        public event EventHandler GameInitialized
        {
            add
            {
                gameInitializedInternal += value;
                if (Status != GameStatus.NotStarted)
                {
                    value(this, EventArgs.Empty);
                }
            }
            remove { gameInitializedInternal -= value; }
        }

        public event EventHandler GameStarted;
        public event EventHandler GameFinished;
        public event EventHandler GameStopped;
        public event EventHandler<TimeSpan> GameUpdate;
        
        string[] ModuleIds { get; }

        protected GameBase(params string[] moduleIds)
        {
            ModuleIds = moduleIds ?? Array.Empty<string>();
        }

        public IServiceResolver ServiceResolver { get; private set; }
        public GameStatus Status { get; protected set; }
        protected IGameLoop GameLoop { get; private set; }
        protected virtual IServiceResolver CreateServiceResolver() => new DefaultServiceResolver();
        
        public void InitializeSystems(ITimeSourceDefinition timeSourceDefinition = null)
        {
            Logger.Debug("Starting");
            timeSourceDefinition ??= new RealTimeSourceDefinition(30);
            
            ServiceResolver = CreateServiceResolver();
            ServiceResolver.Store(timeSourceDefinition);
            InitializeServices(ServiceResolver);

            var ms = CreateModuleSystem();

            GameLoop = timeSourceDefinition.BuildTimeStepLoop(ms.Initialize());

            ServiceResolver.Store(GameLoop.TimeSource);
            ServiceResolver.ValidatePromisesCanResolve();
            LateInitializeSystemsOverride();

            Status = GameStatus.Initialized;
            gameInitializedInternal?.Invoke(this, EventArgs.Empty);
        }

        protected virtual ModuleSystem CreateModuleSystem()
        {
            var ms = new ModuleSystem(ServiceResolver);
            ms.ScanForModules(ModuleIds);
            return ms;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GameLoop?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~GameBase()
        {
            Dispose(false);
        }

        protected virtual void InitializeServices(IServiceResolver serviceResolver)
        {
        }
        
        protected virtual void LateInitializeSystemsOverride()
        {
        }

        protected void FireGameStartedEvent()
        {
            GameStarted?.Invoke(this, EventArgs.Empty);
        }
        
                
        public void Update(TimeSpan absoluteGameTime)
        {
            if (Status != GameStatus.Running)
            {
                return;
            }

            GameLoop.Update(absoluteGameTime);
            UpdateOverride(absoluteGameTime);
            GameUpdate?.Invoke(this, absoluteGameTime);

            var status = CheckStatus();
            if (status.IsFinished())
            {
                Status = status;
                GameFinished?.Invoke(this, EventArgs.Empty);
            }
        }

        public ITimeSource Time => GameLoop.TimeSource;
        
        protected virtual GameStatus CheckStatus() => GameLoop.IsRunning ? GameStatus.Running : Status;
                
        protected virtual void UpdateOverride(TimeSpan absoluteGameTime)
        {
        }

        public virtual void Stop()
        {
            if (!Status.IsStoppable())
            {
                return;
            }

            GameLoop.Stop();
            Status = GameStatus.Initialized;
            GameStopped?.Invoke(this, EventArgs.Empty);
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
    }
}
