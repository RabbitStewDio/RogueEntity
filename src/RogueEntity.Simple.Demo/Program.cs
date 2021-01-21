using Microsoft.Extensions.Configuration;
using RogueEntity.Simple.Demo.BoxPusher;
using SadConsole;
using Serilog;
using Game = SadConsole.Game;

namespace RogueEntity.Simple.Demo
{
    class Program
    {
        static void SetUpLogging()
        {
            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", true)
                                .Build();

            var logger = new LoggerConfiguration()
                         .ReadFrom.Configuration(configuration)
                         .CreateLogger();
            Log.Logger = logger;
        }

        static void Main()
        {
            SetUpLogging();

            var game = new BoxPusherGame(); 

            // Setup the engine and create the main window.
            Game.Create(80, 25);

            // Hook the start event so we can add consoles to the system.
            Game.OnInitialize += () => Init(game);

            // Start the game.
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        static void Init(BoxPusherGame game)
        {
            var console = Global.CurrentScreen;
            console.Clear();

            Global.CurrentScreen = console;
            
            Settings.ResizeMode = Settings.WindowResizeOptions.Fit;
            
            var gameConsole = game.Initialize(console.Width, console.Height);
            console.Children.Add(gameConsole);

        }

    }
}
