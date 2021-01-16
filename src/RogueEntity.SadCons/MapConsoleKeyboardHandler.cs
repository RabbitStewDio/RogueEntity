using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Players;
using SadConsole.Components;
using SadConsole.Input;
using System;
using Console = SadConsole.Console;

namespace RogueEntity.SadCons
{
    public class MapConsoleKeyboardHandler<TPlayerEntity>: KeyboardConsoleComponent
    {
        readonly MapConsoleState sharedConsoleState;
        readonly IPlayerService<TPlayerEntity> playerService;
        Optional<PlayerTag> player;

        public MapConsoleKeyboardHandler([NotNull] MapConsoleState sharedConsoleState,
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

        public override void ProcessKeyboard(Console console, Keyboard info, out bool handled)
        {
            handled = console.IsFocused;

            if (!player.TryGetValue(out var playerTag))
            {
                return;
            }
            
            
        }
    }
}
