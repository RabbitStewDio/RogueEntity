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

            var context = new BoxPusherContext();

            var serviceResolver = new DefaultServiceResolver().WithService(context);

            var ms = new ModuleSystem<BoxPusherContext>(serviceResolver);
            ms.ScanForModules("BoxPusher");

            var e = ms.Initialize(context).BuildRealTimeStepLoop(30);
            serviceResolver.Store(e.TimeSource);

            serviceResolver.ValidatePromisesCanResolve();
        }
    }
}