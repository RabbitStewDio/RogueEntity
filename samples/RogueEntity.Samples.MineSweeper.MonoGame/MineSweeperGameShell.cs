using Microsoft.Xna.Framework;
using RogueEntity.SadCons;
using RogueEntity.Samples.MineSweeper.Core;
using RogueEntity.Samples.MineSweeper.Core.Services;
using System;

namespace RogueEntity.Samples.MineSweeper.MonoGame
{
    public class MineSweeperGameShell : GameShell<MainMenuConsoleContext>
    {
        readonly MineSweeperGame game;
        MineSweeperNewGameContext newGameScreen;
        MineSweeperMapContext mapContext;

        public MineSweeperGameShell()
        {
            game = new MineSweeperGame();
            game.GameStarted += OnGameStarted;
            game.GameStopped += OnGameStopped;
        }

        void OnGameStarted(object sender, EventArgs e)
        {
            ControlsCanvas.IsVisible = false;
        }

        void OnGameStopped(object sender, EventArgs e)
        {
            ControlsCanvas.IsVisible = true;
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

            mapContext = new MineSweeperMapContext(game);
            mapContext.Initialize(this);
            ParentConsole.Children.Add(mapContext.Console);
            
            return canvas;
        }

        void OnNewGameRequested(object sender, MineSweeperGameParameter e)
        {
            game.StartGame(e);
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
            game.Update(time.TotalGameTime);
        }
    }
}
