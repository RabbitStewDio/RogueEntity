using RogueEntity.Api.Services;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Sources.Heat;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public static class InfraVisionSenseReceptorPhysics
    {
        public static IInfraVisionSenseReceptorPhysicsConfiguration GetOrCreateInfraVisionSensorPhysics(this IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve<IInfraVisionSenseReceptorPhysicsConfiguration>(out var physics))
            {
                var physicsConfig = serviceResolver.Resolve<IHeatPhysicsConfiguration>();
                if (!serviceResolver.TryResolve<ShadowPropagationResistanceDataSource>(out var ds))
                {
                    ds = new ShadowPropagationResistanceDataSource();
                    serviceResolver.Store(ds);
                }

                physics = new InfraVisionSenseReceptorPhysicsConfiguration(physicsConfig, ds);
                serviceResolver.Store(physics);
            }

            return physics;
        }
    }
}