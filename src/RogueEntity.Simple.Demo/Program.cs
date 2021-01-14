using Microsoft.Extensions.Configuration;
using Microsoft.Xna.Framework;
using RogueEntity.Simple.Demo.BoxPusher;
using Serilog;
using Console = SadConsole.Console;
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
            
            BoxPusherGame.BoxMain();
            
            // Setup the engine and create the main window.
            Game.Create(80, 25);

            // Hook the start event so we can add consoles to the system.
            Game.OnInitialize = Init;

            // Start the game.
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        static void Init()
        {
            var console = new Console(80, 25);
            console.FillWithRandomGarbage();
            console.Fill(new Rectangle(3, 3, 23, 3), Color.Violet, Color.Black, 0, 0);
            console.Print(4, 4, "Hello from SadConsole");

            SadConsole.Global.CurrentScreen = console;
        }
    }
}