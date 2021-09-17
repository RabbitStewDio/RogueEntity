using Microsoft.Xna.Framework;
using RogueEntity.Core.Players;
using RogueEntity.Core.Storage;
using RogueEntity.SadCons;
using RogueEntity.Samples.BoxPusher.Core;
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
        BoxPusherMapContext mapContext;

        public BoxPusherGameShell()
        {
            game = new BoxPusherGame(DefaultStorageLocationService.CreateDefault("BoxPusher"));
            game.GameStarted += OnGameStarted;
            game.GameStopped += OnGameStopped;
            
            itemTheme = new BoxPusherProfileItemTheme(3);
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

            mapContext = new BoxPusherMapContext(game);
            mapContext.Initialize(this);
            ParentConsole.Children.Add(mapContext.Console);

            return canvas;
        }

        void OnNewProfileCreated(object sender, (Guid profileId, BoxPusherPlayerProfile profile) valueTuple)
        {
            // at this call we can assume that the profile exists, so we can just load the game
            game.StartGame(valueTuple.profileId);
        }

        void OnLoadRequested(object sender, PlayerProfileContainer<BoxPusherPlayerProfile> e)
        {
            loadGameScreen.Hide();
            game.StartGame(e.Id);
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
