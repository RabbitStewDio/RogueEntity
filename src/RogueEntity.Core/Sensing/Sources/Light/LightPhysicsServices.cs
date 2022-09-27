using RogueEntity.Api.Services;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public static class LightPhysicsServices
    {
        public static IServiceResolver ConfigureLightPhysics(this IServiceResolver serviceResolver, DistanceCalculation d = DistanceCalculation.Euclid)
        {
            if (!serviceResolver.TryResolve<ILightPhysicsConfiguration>(out var physicsConfiguration))
            {
                if (!serviceResolver.TryResolve<ShadowPropagationResistanceDataSource>(out var ds))
                {
                    ds = new ShadowPropagationResistanceDataSource();
                    serviceResolver.Store(ds);
                }

                physicsConfiguration = new LightPhysicsConfiguration(LinearDecaySensePhysics.For(d), ds);
                serviceResolver.Store(physicsConfiguration);
            }

            return serviceResolver;
        }
    }
}
