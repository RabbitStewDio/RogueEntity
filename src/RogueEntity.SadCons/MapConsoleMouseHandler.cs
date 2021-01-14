using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Players;
using RogueEntity.Core.Utils;
using SadConsole.Components;
using SadConsole.Input;

namespace RogueEntity.SadCons
{
    public class MapConsoleMouseHandler<TPlayerEntity> : MouseConsoleComponent
        where TPlayerEntity : IEntityKey
    {
        readonly MapConsoleState sharedConsoleState;
        readonly IPlayerService<TPlayerEntity> playerService;
        Optional<PlayerTag> player;

        public MapConsoleMouseHandler(MapConsoleState sharedConsoleState,
                                      IPlayerService<TPlayerEntity> playerService)
        {
            this.sharedConsoleState = sharedConsoleState;
            this.playerService = playerService;
        }

        public void OnPlayerActivated(PlayerTag playerId)
        {
            this.player = playerId;
        }

        public void OnPlayerDeactivated()
        {
            this.player = Optional.Empty();
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