using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
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

        protected override void RegisterCalculateDirectionalSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                          IGameLoopSystemRegistration<TGameContext> context,
                                                                                          EntityRegistry<TItemId> registry)
        {
            RegisterCalculateOmniDirectionalSystem(in initParameter, context, registry);
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            var physics = serviceResolver.GetOrCreateInfraVisionSensorPhysics();
            return (physics.CreateInfraVisionSensorPropagationAlgorithm(), physics.InfraVisionPhysics);
        }
    }
}