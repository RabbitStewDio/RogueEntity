using RogueEntity.Api.Services;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Touch
{
    public static class TouchPhysicsServices
    {
        public static IServiceResolver ConfigureTouchPhysics(this IServiceResolver serviceResolver, DistanceCalculation d = DistanceCalculation.Euclid)
        {
            if (!serviceResolver.TryResolve(out ITouchReceptorPhysicsConfiguration physicsConfiguration))
            {
                if (!serviceResolver.TryResolve(out FloodFillWorkingDataSource ds))
                {
                    ds = new FloodFillWorkingDataSource();
                    serviceResolver.Store(ds);
                }

                physicsConfiguration = new TouchSenseReceptorPhysicsConfiguration(LinearDecaySensePhysics.For(d), ds);
                serviceResolver.Store(physicsConfiguration);
            }

            return serviceResolver;
        }
    }
}
