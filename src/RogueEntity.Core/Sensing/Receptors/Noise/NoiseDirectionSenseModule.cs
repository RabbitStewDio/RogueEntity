using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
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

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out INoiseSenseReceptorPhysicsConfiguration physics))
            {
                var physicsConfig = serviceResolver.Resolve<INoisePhysicsConfiguration>();
                if (!serviceResolver.TryResolve(out FloodFillWorkingDataSource ds))
                {
                    ds = new FloodFillWorkingDataSource();
                }

                physics = new NoiseSenseReceptorPhysicsConfiguration(physicsConfig, ds);
            }

            return (physics.CreateNoiseSensorPropagationAlgorithm(), physics.NoisePhysics);
        }
    }
}