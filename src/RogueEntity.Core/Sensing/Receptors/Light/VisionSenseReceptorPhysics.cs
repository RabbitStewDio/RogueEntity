using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public static class VisionSenseReceptorPhysics
    {
        public static IVisionSenseReceptorPhysicsConfiguration GetOrCreateVisionSensorPhysics(this IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out IVisionSenseReceptorPhysicsConfiguration physics))
            {
                var physicsConfig = serviceResolver.Resolve<ILightPhysicsConfiguration>();
                if (!serviceResolver.TryResolve(out ShadowPropagationResistanceDataSource ds))
                {
                    ds = new ShadowPropagationResistanceDataSource();
                    serviceResolver.Store(ds);
                }

                physics = new VisionSenseReceptorPhysicsConfiguration(physicsConfig, ds);
                serviceResolver.Store(physics);
            }

            return physics;
        }
    }
}