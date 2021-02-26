using Microsoft.Xna.Framework;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Players;
using RogueEntity.Core.Runtime;
using RogueEntity.SadCons;
using RogueEntity.Samples.BoxPusher.Core;
using SadConsole;
using Console = SadConsole.Console;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherMapContext : ConsoleContext<ContainerConsole>
    {
        readonly BoxPusherInputState sharedUIState;
        readonly BoxPusherGame game;
        BoxPusherGameUIContext uiContext;
        Console mapConsole;

        public BoxPusherMapContext(BoxPusherGame game)
        {
            this.sharedUIState = new BoxPusherInputState();

            this.game = game;
            this.game.GameStarted += (s, e) => OnGameStarted();
            this.game.GameStopped += (s, e) => OnGameStopped();
            this.game.GameFinished += (s, e) => OnGameOver();
            this.game.GameUpdate += (s, t) => OnGameUpdate();
        }

        void OnGameUpdate()
        {
            sharedUIState.PlayerObserver = RefreshPlayerObserver();
        }

        Optional<PlayerObserver> RefreshPlayerObserver()
        {
            if ((game.Status & GameStatusMasks.GameActiveMask) == 0)
            {
                // game is neither running, won or lost.
                return Optional.Empty();
            }

            if (game.PlayerData.TryGetValue(out var player) &&
                game.PlayerService.TryQueryPrimaryObserver(player.Tag, out var observer))
            {
                return observer;
            }

            return Optional.Empty();
        }

        void OnGameStarted()
        {
            Console.IsVisible = true;
        }

        void OnGameOver()
        {
            uiContext.ShowQuitDialog();
        }

        void OnGameStopped()
        {
            Console.IsVisible = false;
        }

        public override void Initialize(IConsoleParentContext parentContext)
        {
            base.Initialize(parentContext);
            Console = new ContainerConsole();
            Console.IsVisible = false;

            mapConsole = new Console(parentContext.ScreenBounds.Width / 2, parentContext.ScreenBounds.Height / 2);
            mapConsole.Font = mapConsole.Font.Master.GetFont(Font.FontSizes.Two); 
            mapConsole.Components.Add(new BoxPusherMouseHandler(game, sharedUIState));
            mapConsole.Components.Add(new BoxPusherMapDrawHandler(game, sharedUIState));
            mapConsole.Components.Add(new BoxPusherKeyboardHandler(game, sharedUIState));
            Console.Children.Add(mapConsole);

            uiContext = new BoxPusherGameUIContext(game);
            AddChildContext(uiContext);
        }

        protected override void OnParentConsoleResized()
        {
            base.OnParentConsoleResized();
            if (!game.Status.IsStoppable())
            {
                return;
            }

            // need to resize the viewport to match the game play area
            mapConsole.Resize(ParentContext.ScreenBounds.Width / 2, ParentContext.ScreenBounds.Height / 2, true);
            mapConsole.Position = new Point(0, 0);
        }
    }
}
