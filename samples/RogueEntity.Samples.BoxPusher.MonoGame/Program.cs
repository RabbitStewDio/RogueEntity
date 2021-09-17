using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using Game = SadConsole.Game;

namespace RogueEntity.Samples.BoxPusher.MonoGame
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

            Console.WriteLine("Starting ..");
            
            // Setup the engine and create the main window.
            Game.Create(80, 25);

            var shell = new BoxPusherGameShell();
            Game.OnInitialize += shell.Initialize;
            Game.OnUpdate += shell.Update;
            Game.OnDraw += shell.Draw;
            Game.OnDestroy += shell.Destroy;

            // Start the game.
            Game.Instance.Run();
            Game.Instance.Dispose();
        }
    }
}
