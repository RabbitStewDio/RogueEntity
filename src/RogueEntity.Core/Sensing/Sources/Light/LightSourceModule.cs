using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    [Module]
    public class LightSourceModule : SenseSourceModuleBase<VisionSense, LightSourceDefinition>
    {
        public static readonly string ModuleId = "Core.Senses.Source.Light";

        public LightSourceModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Sense Source - Light";
            Description = "Provides sense sources and sense resistance for visible light";
            IsFrameworkModule = true;
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics(IServiceResolver resolver)
        {
            var physics = resolver.Resolve<ILightPhysicsConfiguration>();
            return (physics.CreateLightPropagationAlgorithm(), physics.LightPhysics);
        }
    }
}