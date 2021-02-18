using RogueEntity.SadCons;
using SadConsole;
using SadConsole.Components;
using SadConsole.Controls;
using System;
using Console = SadConsole.Console;

namespace RogueEntity.Samples.MineSweeper.MonoGame
{
    public class MineSweeperGameUIContext: ConsoleContext<ControlsConsole>
    {
        readonly MineSweeperGame game;
        Window gameOverWindow;
        Window quitConfirmWindow;
        Label statusLabel;
        Label gameOverMessage;

        public MineSweeperGameUIContext(MineSweeperGame game)
        {
            this.game = game;
        }

        public override void Initialize(IConsoleParentContext parentContext)
        {
            base.Initialize(parentContext);

            Console = new ControlsConsole(parentContext.Bounds.Width, 1);
            Console.FocusOnMouseClick = false;
            Console.IsVisible = true;
            
            statusLabel = SadConsoleControls.CreateLabel("Position: [99, 99]").WithPlacementAt(0, 0);
            Console.Add(statusLabel);

            gameOverMessage = SadConsoleControls.CreateLabel("Game Over!");
            
            gameOverWindow = new Window(40, 20);
            gameOverWindow.Center();
            gameOverWindow.Add(gameOverMessage.WithPlacementAt(5, 5));
            gameOverWindow.Add(SadConsoleControls.CreateButton("Back to Main Menu", 20, 3).WithAction(OnBackToMenu).WithPlacementAt(5, 12));

            quitConfirmWindow = new Window(40, 20);
            quitConfirmWindow.Center();
            quitConfirmWindow.Add(SadConsoleControls.CreateLabel("Really Quit?").WithPlacementAt(5, 5));
            quitConfirmWindow.Add(SadConsoleControls.CreateButton("Quit", 10, 3).WithAction(OnBackToMenu).WithPlacementAt(4, 12));
            quitConfirmWindow.Add(SadConsoleControls.CreateButton("Cancel", 10, 3).WithAction(OnCancelQuit).WithPlacementAt(16, 12));
        }

        public void ShowQuitDialog()
        {
            quitConfirmWindow.Show(true);
        }

        public void ShowGameOverDialog()
        {
            if (game.Status == MineSweeperGameStatus.GameLost)
            {
                gameOverMessage.DisplayText = "Game Over!";
            }
            else
            {
                gameOverMessage.DisplayText = "You won!";
            }
            
            gameOverWindow.Show(true);
        }

        protected override void OnParentConsoleResized()
        {
            base.OnParentConsoleResized();
            gameOverWindow.Center();
            quitConfirmWindow.Center();
        }

        void OnCancelQuit()
        {
            quitConfirmWindow.Hide();
        }

        void OnBackToMenu()
        {
            game.Stop();
            quitConfirmWindow.Hide();
            gameOverWindow.Hide();
        }
    }
}
