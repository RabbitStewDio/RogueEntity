using RogueEntity.Api.Services;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    public static class SmellPhysicsServices
    {
        public static IServiceResolver ConfigureSmellPhysics(this IServiceResolver serviceResolver, DistanceCalculation d = DistanceCalculation.Euclid)
        {
            if (!serviceResolver.TryResolve<ISmellPhysicsConfiguration>(out var physicsConfiguration))
            {
                if (!serviceResolver.TryResolve<FloodFillWorkingDataSource>(out var ds))
                {
                    ds = new FloodFillWorkingDataSource();
                    serviceResolver.Store(ds);
                }

                physicsConfiguration = new SmellPhysicsConfiguration(LinearDecaySensePhysics.For(d), ds);
                serviceResolver.Store(physicsConfiguration);
            }

            return serviceResolver;
        }
    }
}
