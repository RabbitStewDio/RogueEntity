using Microsoft.Xna.Framework;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Services;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Players;
using RogueEntity.SadCons;
using SadConsole;
using Serilog;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherGame
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        readonly DirectoryCatalog pluginCatalogue;

        ActorReference currentPlayerEntity;
        PlayerTag currentPlayerTag;
        IGameLoop gameLoop;
        MapConsole mapConsole;

        public IPlayerService<ActorReference> PlayerService { get; private set; }
        public IServiceResolver ServiceResolver { get; private set; }

        public BoxPusherGame()
        {
            pluginCatalogue = new DirectoryCatalog(".");
        }
        
        public Console Initialize(int width, int height)
        {
            Log.Debug("Starting");
            ServiceResolver = new DefaultServiceResolver();

            var ms = new ModuleSystem(ServiceResolver);
            ms.ScanForModules("BoxPusher");

            gameLoop = ms.Initialize().BuildRealTimeStepLoop(30);
            ServiceResolver.Store(gameLoop.TimeSource);
            ServiceResolver.ValidatePromisesCanResolve();

            PlayerService = ServiceResolver.Resolve<IPlayerService<ActorReference>>();

            var console = new SadConsole.Console(width, height);
            console.FillWithRandomGarbage();
            
            mapConsole = new MapConsole(console);
            mapConsole.Initialize(PlayerService);
            
            return console;
        }

        public void Start()
        {
            gameLoop.Initialize();
            
            (currentPlayerTag, currentPlayerEntity) = PlayerService.GetOrCreate(PlayerIds.SinglePlayer);
        }

        public void Stop()
        {
            gameLoop.Dispose();
        }
        
        public void Update(GameTime time)
        {
            gameLoop.Update(time.ElapsedGameTime);
        } 
    }
}