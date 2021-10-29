using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Grid;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RogueEntity.Samples.BoxPusher.Core
{
    public class BoxPusherPlayerStatusService: INotifyPropertyChanged
    {
        readonly BoxPusherGame game;
        readonly IItemResolver<ActorReference> actorResolver;
        EntityGridPosition position;
        bool notifiedWin;
        int currentLevel;

        public BoxPusherPlayerStatusService(BoxPusherGame game, IItemResolver<ActorReference> actorResolver)
        {
            this.game = game;
            this.actorResolver = actorResolver;
            this.game.GameUpdate += OnGameUpdate;
        }


        public EntityGridPosition Position
        {
            get => position;
            private set
            {
                if (value.Equals(position)) return;
                position = value;
                OnPropertyChanged();
            }
        }

        public int CurrentLevel
        {
            get => currentLevel;
            set
            {
                if (value == currentLevel) return;
                currentLevel = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler LevelCleared;
        public event PropertyChangedEventHandler PropertyChanged;

        void OnGameUpdate(object sender, TimeSpan e)
        {
            if (game.PlayerData.TryGetValue(out var player) && 
                actorResolver.TryQueryData(player.EntityId, out EntityGridPosition pos))
            {
                Position = pos;
            }
            else
            {
                Position = EntityGridPosition.Invalid;
            }

            if (game.CurrentPlayerProfile.TryGetValue(out var profile))
            {
                if (profile.CurrentLevel != CurrentLevel)
                {
                    CurrentLevel = profile.CurrentLevel;
                    notifiedWin = false;
                }
                
                if (!notifiedWin && profile.IsCurrentLevelComplete())
                {
                    // show winning screen.
                    LevelCleared?.Invoke(this, EventArgs.Empty);
                    notifiedWin = true;
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
