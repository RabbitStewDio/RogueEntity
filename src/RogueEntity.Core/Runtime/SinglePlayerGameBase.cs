using RogueEntity.Api.Utils;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.Players;
using Serilog;
using System;

namespace RogueEntity.Core.Runtime
{
    public abstract class SinglePlayerGameBase<TPlayerEntity>: GameBase
    {
        static readonly ILogger Logger = SLog.ForContext<SinglePlayerGameBase<TPlayerEntity>>();

        protected SinglePlayerGameBase(params string[] moduleIds): base(moduleIds)
        {
            GameFinished += (_,_) => RemoveRemainingActivePlayer();
            GameStopped += (_,_) => RemoveRemainingActivePlayer();
        }

        public IPlayerService PlayerService { get; private set; }
        public IPlayerManager<TPlayerEntity> PlayerManager { get; private set; }
        public Optional<PlayerReference<TPlayerEntity>> PlayerData { get; private set; }
        public IBasicCommandService<TPlayerEntity> CommandService { get; private set; }

        protected override void LateInitializeSystemsOverride()
        {
            PlayerService = ServiceResolver.Resolve<IPlayerService>();
            PlayerManager = ServiceResolver.Resolve<IPlayerManager<TPlayerEntity>>();
            PlayerManager.PlayerActivated += OnPlayerActivated;
            PlayerManager.PlayerDeactivated += OnPlayerDeactivated;
            CommandService = ServiceResolver.Resolve<IBasicCommandService<TPlayerEntity>>();
        }

        void OnPlayerActivated(object sender, PlayerReference<TPlayerEntity> e)
        {
            if (!PlayerData.HasValue)
            {
                PlayerData = e;
            }
        }

        void OnPlayerDeactivated(object sender, PlayerReference<TPlayerEntity> e)
        {
            if (PlayerData.TryGetValue(out var existing) &&
                existing.Tag == e.Tag)
            {
                PlayerData = default;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            PlayerManager.PlayerActivated -= OnPlayerActivated;
            PlayerManager.PlayerDeactivated -= OnPlayerDeactivated;
        }

        protected virtual bool StartGameWithPlayer(Guid playerId = default)
        {
            if (Status != GameStatus.Initialized)
            {
                return false;
            }

            GameLoop.Initialize(IsBlockedOrWaitingForInput);
            
            if (ActivatePlayer(playerId))
            {
                
                Status = GameStatus.Running;
                FireGameStartedEvent();
                return true;
            }
            
            return false;
        }

        protected virtual bool StartGameWithoutPlayer()
        {
            if (Status != GameStatus.Initialized)
            {
                return false;
            }

            GameLoop.Initialize(IsBlockedOrWaitingForInput);
                
            Status = GameStatus.Running;
            FireGameStartedEvent();
            return true;

        }

        protected virtual bool ActivatePlayer(Guid playerId = default)
        {
            if (playerId == default)
            {
                playerId = PlayerIds.SinglePlayer;
            }

            if (PlayerManager.TryActivatePlayer(playerId, out var playerTag, out var playerEntityId))
            {
                PlayerData = new PlayerReference<TPlayerEntity>(playerTag, playerEntityId);
                return true;
            }

            return false;
        }

        protected virtual bool DeactivatePlayer()
        {
            if (PlayerData.TryGetValue(out var playerData))
            {
                if (PlayerManager.TryDeactivatePlayer(playerData.Tag.Id))
                {
                    PlayerData = default;
                }
            }

            return false;
        }
        
        void RemoveRemainingActivePlayer()
        {
            if (PlayerData.TryGetValue(out var playerData) &&
                !PlayerManager.TryDeactivatePlayer(playerData.Tag.Id))
            {
                // Big warning, unable to kill player.
                Logger.Warning("Unable to remove player while disposing game state");
            }
            
            PlayerData = default;
        }
    }
}
