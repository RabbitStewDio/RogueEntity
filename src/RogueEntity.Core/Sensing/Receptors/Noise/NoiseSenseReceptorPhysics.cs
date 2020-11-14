using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Sources.Noise;

namespace RogueEntity.Core.Sensing.Receptors.Noise
{
    public static class NoiseSenseReceptorPhysics
    {
        
        public static INoiseSenseReceptorPhysicsConfiguration GetOrCreateNoiseSensorPhysics(this IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out INoiseSenseReceptorPhysicsConfiguration physics))
            {
                var physicsConfig = serviceResolver.Resolve<INoisePhysicsConfiguration>();
                if (!serviceResolver.TryResolve(out FloodFillWorkingDataSource ds))
                {
                    ds = new FloodFillWorkingDataSource();
                    serviceResolver.Store(ds);
                }

                physics = new NoiseSenseReceptorPhysicsConfiguration(physicsConfig, ds);
                serviceResolver.Store(physics);
            }

            return physics;
        }
    }
}