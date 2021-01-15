using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Core.Infrastructure.Services;
using Serilog;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherGame
    {
        public static void BoxMain()
        {
            Log.Debug("Starting");

            var serviceResolver = new DefaultServiceResolver();

            var ms = new ModuleSystem(serviceResolver);
            ms.ScanForModules("BoxPusher");

            var e = ms.Initialize().BuildRealTimeStepLoop(30);
            serviceResolver.Store(e.TimeSource);

            serviceResolver.ValidatePromisesCanResolve();
        }
    }
}