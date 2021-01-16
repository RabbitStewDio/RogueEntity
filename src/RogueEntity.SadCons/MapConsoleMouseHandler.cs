using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Players;
using RogueEntity.Core.Utils;
using SadConsole.Components;
using SadConsole.Input;
using System;

namespace RogueEntity.SadCons
{
    public class MapConsoleMouseHandler<TPlayerEntity> : MouseConsoleComponent
        where TPlayerEntity : IEntityKey
    {
        readonly MapConsoleState sharedConsoleState;
        readonly IPlayerService<TPlayerEntity> playerService;
        Optional<PlayerTag> player;

        public MapConsoleMouseHandler([NotNull] MapConsoleState sharedConsoleState,
                                      [NotNull] IPlayerService<TPlayerEntity> playerService)
        {
            this.sharedConsoleState = sharedConsoleState ?? throw new ArgumentNullException(nameof(sharedConsoleState));
            this.playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
            this.playerService.PlayerActivated += OnPlayerActivated;
            this.playerService.PlayerDeactivated += OnPlayerDeactivated;
        }

        void OnPlayerActivated(object sender, PlayerEventArgs<TPlayerEntity> e)
        {
            if (e.PlayerId == sharedConsoleState.PlayerId)
            {
                this.player = e.PlayerId;
            }
        }

        public void OnPlayerDeactivated(object sender, PlayerEventArgs<TPlayerEntity> e)
        {
            if (e.PlayerId == sharedConsoleState.PlayerId)
            {
                this.player = Optional.Empty();
            }
        }

        public override void ProcessMouse(SadConsole.Console console,
                                          MouseConsoleState state,
                                          out bool handled)
        {
            handled = false;

            if (!player.TryGetValue(out var playerId))
            {
                return;
            }

            if (!playerService.TryQueryPrimaryObserver(playerId, out var observer) || 
                observer.Position.IsInvalid)
            {
                return;
            }
            
            var clickPosition = state.CellPosition;
            var viewPort = sharedConsoleState.MapViewPort;
            var mapPosition = viewPort.MinExtent + new Position2D(clickPosition.X, clickPosition.Y);
            
            sharedConsoleState.MouseHoverPosition = observer.Position.WithPosition(mapPosition.X, mapPosition.Y);

            if (!state.IsOnConsole)
            {
                return;
            }

            if (state.Mouse.LeftClicked)
            {
                sharedConsoleState.FireMouseSelection();
            }
            
            if (state.Mouse.RightClicked)
            {
                sharedConsoleState.FireMouseContextMenu();
            }
        }
    }
}