using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Sources.Heat;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    [Module]
    public class InfraVisionSenseModule : SenseReceptorModuleBase<VisionSense, TemperatureSense, HeatSourceDefinition>
    {
        public const string ModuleId = "Core.Sense.Receptor.InfraVision";

        public InfraVisionSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view for heat/infrared vision.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(HeatSourceModule.ModuleId),
                                ModuleDependency.Of(LightSourceModule.ModuleId)
            );
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out IInfraVisionSenseReceptorPhysicsConfiguration physics))
            {
                var physicsConfig = serviceResolver.Resolve<IHeatPhysicsConfiguration>();
                if (!serviceResolver.TryResolve(out ShadowPropagationResistanceDataSource ds))
                {
                    ds = new ShadowPropagationResistanceDataSource();
                }

                physics = new InfraVisionSenseReceptorPhysicsConfiguration(physicsConfig, ds);
            }

            return (physics.CreateInfraVisionSensorPropagationAlgorithm(), physics.InfraVisionPhysics);
        }
    }
}