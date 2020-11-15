using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Sources.Light;
using RogueEntity.Core.Utils.Algorithms;
using Serilog;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public class BoxPusherGame
    {
        public static void BoxMain()
        {
            Log.Debug("Starting");

            var context = new BoxPusherContext();

            var serviceResolver = new DefaultServiceResolver().WithService(context)
                                                              .WithService<ShadowPropagationResistanceDataSource>()
                                                              .WithService<ILightPhysicsConfiguration>(new LightPhysicsConfiguration(new LinearDecaySensePhysics(DistanceCalculation.Euclid)));
                                                              

            var ms = new ModuleSystem<BoxPusherContext>(serviceResolver);
            ms.ScanForModules("BoxPusher");

            var e = ms.Initialize(context).BuildRealTimeStepLoop(30);
            serviceResolver.Store(e.TimeSource);

            serviceResolver.ValidatePromisesCanResolve();
        }
    }
}