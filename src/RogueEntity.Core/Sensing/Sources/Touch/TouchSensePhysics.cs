using RogueEntity.Api.Services;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Touch
{
    public static class TouchSensePhysics
    {
        public static ITouchReceptorPhysicsConfiguration GetOrCreateTouchPhysics(this IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out ITouchReceptorPhysicsConfiguration physics))
            {
                if (!serviceResolver.TryResolve(out FloodFillWorkingDataSource ds))
                {
                    ds = new FloodFillWorkingDataSource();
                    serviceResolver.Store(ds);
                }
                physics = new TouchSenseReceptorPhysicsConfiguration(new FullStrengthSensePhysics(LinearDecaySensePhysics.For(DistanceCalculation.Euclid)), ds);
                serviceResolver.Store(physics);
            }

            return physics;
        }
    }
}