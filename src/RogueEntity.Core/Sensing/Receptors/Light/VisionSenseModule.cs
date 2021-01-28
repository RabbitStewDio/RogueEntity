using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
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

        protected override void RegisterCalculateDirectionalSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                            IGameLoopSystemRegistration context,
                                                                            EntityRegistry<TItemId> registry)
        {
            RegisterCalculateOmniDirectionalSystem(in initParameter, context, registry);
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            var physics = serviceResolver.GetOrCreateVisionSensorPhysics();
            return (physics.CreateVisionSensorPropagationAlgorithm(), physics.VisionPhysics);
        }
    }
}
