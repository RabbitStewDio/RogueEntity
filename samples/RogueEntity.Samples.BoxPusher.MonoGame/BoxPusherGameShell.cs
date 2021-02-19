using Microsoft.Xna.Framework;
using RogueEntity.Core.Players;
using RogueEntity.SadCons;
using RogueEntity.Samples.BoxPusher.Core.ItemTraits;
using System;

namespace RogueEntity.Samples.BoxPusher.MonoGame
{
    public class BoxPusherGameShell : GameShell<MainMenuConsoleContext>
    {
        readonly BoxPusherGame game;
        readonly BoxPusherProfileItemTheme itemTheme;
        LoadGameContext<BoxPusherPlayerProfile> loadGameScreen;
        BoxPusherNewGameContext newGameScreen;

        public BoxPusherGameShell()
        {
            game = new BoxPusherGame();
            itemTheme = new BoxPusherProfileItemTheme(3);
        }

        protected override MainMenuConsoleContext InitializeLateOverride()
        {
            game.InitializeSystems();
            
            var canvas = new MainMenuConsoleContext()
            {
                HasSettings = false,
                HasLoadScreen = true
            };
            canvas.OnQuitGame += OnQuitGame;
            canvas.OnNewGame += OnShowNewGameDialog;
            canvas.OnLoadGame += OnShowLoadGameDialog;
            canvas.OnSettings += OnShowSettingsDialog;
            canvas.Initialize(this);

            loadGameScreen = new LoadGameContext<BoxPusherPlayerProfile>(game.ProfileManager);
            loadGameScreen.ListItemRenderer = itemTheme;
            loadGameScreen.LoadRequested += OnLoadRequested;
            
            newGameScreen = new BoxPusherNewGameContext(game.ProfileManager);
            newGameScreen.Play += OnNewProfileCreated;

            canvas.AddChildContext(newGameScreen);
            canvas.AddChildContext(loadGameScreen);
            return canvas;
        }

        void OnNewProfileCreated(object sender, (Guid, BoxPusherPlayerProfile) valueTuple)
        {
            // at this call we can assume that the profile exists, so we can just load the game
            
        }

        void OnLoadRequested(object sender, PlayerProfileContainer<BoxPusherPlayerProfile> e)
        {
            loadGameScreen.Hide();
            game.StartGame(e.Id);
            ControlsCanvas.IsVisible = false;
        }

        void OnShowSettingsDialog()
        {
            
        }

        void OnShowLoadGameDialog()
        {
            loadGameScreen.Show();
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

        protected IPlayerProfileManager<BoxPusherPlayerProfile> ProfileManager => game.ProfileManager;
    }
}
