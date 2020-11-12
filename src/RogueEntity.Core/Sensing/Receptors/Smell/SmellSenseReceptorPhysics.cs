using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Sources.Smell;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    public static class SmellSenseReceptorPhysics
    {
        public static ISmellSenseReceptorPhysicsConfiguration GetOrCreateSmellSensorPhysics(this IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out ISmellSenseReceptorPhysicsConfiguration physics))
            {
                var physicsConfig = serviceResolver.Resolve<ISmellPhysicsConfiguration>();
                if (!serviceResolver.TryResolve(out FloodFillWorkingDataSource ds))
                {
                    ds = new FloodFillWorkingDataSource();
                    serviceResolver.Store(ds);
                }

                physics = new SmellSenseReceptorPhysicsConfiguration(physicsConfig, ds);
                serviceResolver.Store(physics);
            }

            return physics;
        }
    }
}