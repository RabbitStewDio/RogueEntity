using RogueEntity.Api.Services;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Sources.Heat;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    public static class HeatSenseReceptorPhysics
    {
        public static IHeatSenseReceptorPhysicsConfiguration GetOrCreateHeatSensorPhysics(this IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out IHeatSenseReceptorPhysicsConfiguration physics))
            {
                var physicsConfig = serviceResolver.Resolve<IHeatPhysicsConfiguration>();
                if (!serviceResolver.TryResolve(out FloodFillWorkingDataSource ds))
                {
                    ds = new FloodFillWorkingDataSource();
                    serviceResolver.Store(ds);
                }

                physics = new HeatSenseReceptorPhysicsConfiguration(physicsConfig, ds);
                serviceResolver.Store(physics);
            }

            return physics;
        }
    }
}