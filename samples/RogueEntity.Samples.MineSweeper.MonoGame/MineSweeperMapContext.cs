using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RogueEntity.Core.Runtime;
using RogueEntity.SadCons;
using RogueEntity.Samples.MineSweeper.Core;
using SadConsole;
using SadConsole.Components;
using System;
using Console = SadConsole.Console;
using Keyboard = SadConsole.Input.Keyboard;

namespace RogueEntity.Samples.MineSweeper.MonoGame
{
    public class MineSweeperMapContext: ConsoleContext<ContainerConsole>
    {
        readonly MineSweeperGame game;
        readonly MineSweeperInputState inputState;
        MineSweeperGameUIContext uiContext;
        ScrollingConsole mapConsole;

        public MineSweeperMapContext(MineSweeperGame game)
        {
            this.game = game;
            this.game.GameStarted += (s,e) => OnGameStarted();
            this.game.GameStopped += (s,e) => OnGameStopped();
            this.game.GameFinished += (s, e) => OnGameOver();
            this.inputState = new MineSweeperInputState();
        }

        void OnGameOver()
        {
            uiContext.ShowGameOverDialog();
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
            
            mapConsole = new ScrollingConsole(parentContext.Bounds.Width, parentContext.Bounds.Height);
            mapConsole.Components.Add(new MineSweeperMouseHandler(inputState, game));
            mapConsole.Components.Add(new MineSweeperMapDrawHandler(inputState, game));
            mapConsole.Components.Add(new MineSweeperKeyboardHandler(this));
            Console.Children.Add(mapConsole);

            uiContext = new MineSweeperGameUIContext(game);
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
            var playerData = game.GameParameterService.WorldParameter;
            var gameArea = playerData.PlayFieldBounds;
            var visibleAra = ParentContext.ScreenBounds;
            var viewOrigin = mapConsole.ViewPort.Center;
            
            var sizeX = Math.Min(visibleAra.Width, gameArea.Width);
            var sizeY = Math.Min(visibleAra.Height, gameArea.Height);
            var viewPortRect = new Rectangle(0, 0, sizeX, sizeY);
            viewPortRect.CenterOnPoint(viewOrigin, gameArea.Width, gameArea.Height);
            mapConsole.Resize(gameArea.Width, gameArea.Height, true, viewPortRect);
            mapConsole.Position = new Point
            {
                X = Math.Max(0, (visibleAra.Width - mapConsole.Width) / 2), 
                Y = Math.Max(0, (visibleAra.Height - mapConsole.Height) / 2)
            };
            
            System.Console.WriteLine("Render Console: " + viewPortRect + " at " + mapConsole.Position);
        }

        void OnGameStarted()
        {
            Console.IsVisible = true;
            
            var playerData = game.GameParameterService.WorldParameter;
            var gameArea = playerData.PlayFieldBounds;
            var visibleAra = ParentContext.ScreenBounds;

            var sizeX = Math.Min(visibleAra.Width, gameArea.Width);
            var sizeY = Math.Min(visibleAra.Height, gameArea.Height);
            var viewPortRect = new Rectangle(0, 0, sizeX, sizeY);
            viewPortRect.CenterOnPoint(new Point(gameArea.Center.X, gameArea.Center.Y), gameArea.Width, gameArea.Height);
            mapConsole.Resize(gameArea.Width, gameArea.Height, true, viewPortRect);
            mapConsole.Position = new Point
            {
                X = Math.Max(0, (visibleAra.Width - mapConsole.Width) / 2), 
                Y = Math.Max(0, (visibleAra.Height - mapConsole.Height) / 2)
            };
            
            System.Console.WriteLine("Render Console: " + viewPortRect + " at " + mapConsole.Position);
        }

        class MineSweeperKeyboardHandler : KeyboardConsoleComponent
        {
            readonly MineSweeperMapContext mineSweeperMapContext;

            public MineSweeperKeyboardHandler(MineSweeperMapContext mineSweeperMapContext)
            {
                this.mineSweeperMapContext = mineSweeperMapContext;
            }

            public override void ProcessKeyboard(Console console, Keyboard info, out bool handled)
            {
                handled = true;
                if (info.IsKeyPressed(Keys.Escape))
                {
                    mineSweeperMapContext.uiContext.ShowQuitDialog();
                }
            }
        }
        
    }
}
