using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    [Module]
    public class VisionSenseModule : SenseReceptorModuleBase<VisionSense, VisionSense, LightSourceDefinition>
    {
        public const string ModuleId = "Core.Sense.Receptor.Vision";

        public VisionSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(LightSourceModule.ModuleId));
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out IVisionSenseReceptorPhysicsConfiguration physics))
            {
                var physicsConfig = serviceResolver.Resolve<ILightPhysicsConfiguration>();
                if (!serviceResolver.TryResolve(out ShadowPropagationResistanceDataSource ds))
                {
                    ds = new ShadowPropagationResistanceDataSource();
                }

                physics = new VisionSenseReceptorPhysicsConfiguration(physicsConfig, ds);
            }

            return (physics.CreateVisionSensorPropagationAlgorithm(), physics.VisionPhysics);
        }
    }
}