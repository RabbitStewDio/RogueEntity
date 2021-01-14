using RogueEntity.Api.Services;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public static class NoisePhysicsServices
    {
        public static IServiceResolver ConfigureNoisePhysics(this IServiceResolver serviceResolver, DistanceCalculation d = DistanceCalculation.Euclid)
        {
            if (!serviceResolver.TryResolve(out INoisePhysicsConfiguration physicsConfiguration))
            {
                if (!serviceResolver.TryResolve(out FloodFillWorkingDataSource ds))
                {
                    ds = new FloodFillWorkingDataSource();
                    serviceResolver.Store(ds);
                }

                physicsConfiguration = new NoisePhysicsConfiguration(LinearDecaySensePhysics.For(d), ds);
                serviceResolver.Store(physicsConfiguration);
            }

            return serviceResolver;
        }

    }
}
