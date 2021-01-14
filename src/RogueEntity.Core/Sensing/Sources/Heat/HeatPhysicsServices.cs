using RogueEntity.Api.Services;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public static class HeatPhysicsServices
    {
        public static IServiceResolver ConfigureLightPhysics(this IServiceResolver serviceResolver, Temperature t, DistanceCalculation d = DistanceCalculation.Euclid)
        {
            if (!serviceResolver.TryResolve(out IHeatPhysicsConfiguration physicsConfiguration))
            {
                if (!serviceResolver.TryResolve(out ShadowPropagationResistanceDataSource ds))
                {
                    ds = new ShadowPropagationResistanceDataSource();
                    serviceResolver.Store(ds);
                }

                physicsConfiguration = new HeatPhysicsConfiguration(LinearDecaySensePhysics.For(d), t, ds);
                serviceResolver.Store(physicsConfiguration);
            }

            return serviceResolver;
        }
        
    }
}
