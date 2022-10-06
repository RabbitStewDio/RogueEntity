using EnTTSharp;
using Microsoft.Xna.Framework;
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
            this.sharedUIState.QuitInitiated += OnQuitInitiated;

            this.game = game;
            this.game.GameStarted += (_, _) => OnGameStarted();
            this.game.GameStopped += (_, _) => OnGameStopped();
            this.game.GameFinished += (_, _) => OnGameOver();
            this.game.GameUpdate += (_, _) => OnGameUpdate();
        }

        void OnQuitInitiated()
        {
            if (uiContext == null)
            {
                return;
            }
            
            uiContext.ShowQuitDialog();
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

            var font = Global.LoadFont("Content/MapTiles.font").GetFont(Font.FontSizes.Two);
            var size = parentContext.ScreenBounds.BoundsFor(font);
            mapConsole = new Console(size.Width, size.Height);
            mapConsole.Font = font; 
            mapConsole.Components.Add(new BoxPusherMouseHandler(game, sharedUIState));
            mapConsole.Components.Add(new BoxPusherKeyboardHandler(game, sharedUIState));
            mapConsole.Components.Add(new BoxPusherMapDrawHandler(game, sharedUIState));
            Console.Children.Add(mapConsole);

            uiContext = new BoxPusherGameUIContext(game);
            AddChildContext(uiContext);

            mapConsole.FocusedMode = SadConsole.Console.ActiveBehavior.Push;
            mapConsole.IsFocused = true;
        }

        protected override void OnParentConsoleResized()
        {
            base.OnParentConsoleResized();
            if (!game.Status.IsStoppable())
            {
                return;
            }

            // need to resize the viewport to match the game play area
            var size = ParentContext.ScreenBounds.BoundsFor(mapConsole.Font);
            mapConsole.Resize(size.Width, size.Height, true);
            mapConsole.Position = new Point(0, 0);
        }
    }
}
