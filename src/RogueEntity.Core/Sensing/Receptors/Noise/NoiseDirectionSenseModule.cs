using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources.Noise;

namespace RogueEntity.Core.Sensing.Receptors.Noise
{
    [Module]
    public class NoiseDirectionSenseModule : SenseReceptorModuleBase<NoiseSense, NoiseSense, NoiseSourceDefinition>
    {
        public const string ModuleId = "Core.Sense.Receptor.Noise";

        public NoiseDirectionSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view for a directional sense of noise.";
            IsFrameworkModule = true;

            DeclareDependency(ModuleDependency.Of(NoiseSourceModule.ModuleId));
        }

        protected override void RegisterCalculateDirectionalSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                                            IGameLoopSystemRegistration context,
                                                                            EntityRegistry<TItemId> registry)
        {
            RegisterCalculateUniDirectionalSystem(in initParameter, context, registry);
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            var physics = serviceResolver.GetOrCreateNoiseSensorPhysics();
            return (physics.CreateNoiseSensorPropagationAlgorithm(), physics.NoisePhysics);
        }
    }
}
