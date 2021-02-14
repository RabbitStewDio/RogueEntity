using Microsoft.Xna.Framework;
using RogueEntity.SadCons;
using RogueEntity.Simple.MineSweeper;

namespace RogueEntity.Samples.MineSweeper.MonoGame
{
    public class MineSweeperGameShell : GameShell<MainMenuConsoleContext>
    {
        readonly MineSweeperGame game;
        MineSweeperNewGameContext newGameScreen;

        public MineSweeperGameShell()
        {
            game = new MineSweeperGame();
        }

        protected override MainMenuConsoleContext InitializeLateOverride()
        {
            game.InitializeSystems();

            var canvas = new MainMenuConsoleContext()
            {
                HasSettings = false,
                HasLoadScreen = false
            };
            canvas.OnQuitGame += OnQuitGame;
            canvas.OnNewGame += OnShowNewGameDialog;
            canvas.OnSettings += OnShowSettingsDialog;
            canvas.Initialize(this);

            newGameScreen = new MineSweeperNewGameContext();
            newGameScreen.NewGameRequested += OnNewGameRequested;

            canvas.AddChildContext(newGameScreen);
            return canvas;
        }

        void OnNewGameRequested(object? sender, MineSweeperPlayerProfile e)
        {
            System.Console.WriteLine($"Starting new game for {e}");
            if (game.ProfileManager.TryCreatePlayer(e, out var guid, out var actualProfile) &&
                game.StartGame(guid))
            {
                ControlsCanvas.IsVisible = false;
            }
        }

        void OnShowSettingsDialog()
        {
            
        }

        void OnShowNewGameDialog()
        {
            newGameScreen.Show();
        }

        void OnQuitGame()
        {
            SadConsole.Game.Instance.Exit();
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            game.Update(time);
            
            if (!game.Started && !ControlsCanvas.IsVisible)
            {
                ControlsCanvas.IsVisible = true;
            }
        }
    }
}
