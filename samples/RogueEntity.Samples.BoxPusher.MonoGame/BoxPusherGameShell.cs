using Microsoft.Xna.Framework;
using RogueEntity.Core.Players;
using RogueEntity.SadCons;
using RogueEntity.Simple.BoxPusher.ItemTraits;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherGameShell : GameShell<MainMenuConsoleContext>
    {
        readonly IPlayerProfileManager<BoxPusherPlayerProfile> profileManager;
        readonly BoxPusherGame game;
        readonly BoxPusherProfileItemTheme itemTheme;
        LoadGameContext<BoxPusherPlayerProfile> loadGameScreen;
        BoxPusherNewGameContext newGameScreen;

        public BoxPusherGameShell()
        {
            game = new BoxPusherGame();
            itemTheme = new BoxPusherProfileItemTheme(3);
        }

        
        LoadGameContext<BoxPusherPlayerProfile> CreateLoadScreen()
        {
            var loadGameContext = new LoadGameContext<BoxPusherPlayerProfile>(game.ProfileManager);
            loadGameContext.ListItemRenderer = itemTheme;
            loadGameContext.LoadRequested += OnLoadRequested;
            return loadGameContext;
        }

        BoxPusherNewGameContext CreateNewGameScreen()
        {
            var loadGameContext = new BoxPusherNewGameContext(game.ProfileManager);
            return loadGameContext;
        }

        protected override void InitializeLateOverride()
        {
            game.InitializeSystems();
            
            base.InitializeLateOverride();
            
            ControlsCanvas = new MainMenuConsoleContext()
            {
                HasSettings = false,
                HasLoadScreen = true
            };
            ControlsCanvas.OnQuitGame += OnQuitGame;
            ControlsCanvas.OnNewGame += OnShowNewGameDialog;
            ControlsCanvas.OnLoadGame += OnShowLoadGameDialog;
            ControlsCanvas.OnSettings += OnShowSettingsDialog;

            
            loadGameScreen = CreateLoadScreen();
            newGameScreen = CreateNewGameScreen();

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

        void OnLoadRequested(object sender, PlayerProfileContainer<BoxPusherPlayerProfile> e)
        {
            loadGameScreen.Hide();
            game.StartGame(e.Id);
            ControlsCanvas.IsVisible = false;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            game.Update(time);
        }

        protected IPlayerProfileManager<BoxPusherPlayerProfile> ProfileManager => game.ProfileManager;
    }
}
