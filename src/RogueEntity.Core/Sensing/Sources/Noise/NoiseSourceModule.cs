using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    [Module]
    public class NoiseSourceModule : SenseSourceModuleBase<NoiseSense, NoiseSourceDefinition>
    {
        public static readonly string ModuleId = "Core.Senses.Source.Noise";
        
        public NoiseSourceModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Sense Source - Noise";
            Description = "Provides sense sources and sense resistance for noise and sound";
            IsFrameworkModule = true;
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics(IServiceResolver resolver)
        {
            var physics = resolver.Resolve<INoisePhysicsConfiguration>();
            return (physics.CreateNoisePropagationAlgorithm(), physics.NoisePhysics);
        }
    }
}