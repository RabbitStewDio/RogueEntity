using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    [Module]
    public class SmellSourceModule : SenseSourceModuleBase<SmellSense, SmellSourceDefinition>
    {
        public static readonly string ModuleId = "Core.Senses.Source.Smell";

        public SmellSourceModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Sense Source - Smell";
            Description = "Provides sense sources and sense resistance for smells";
            IsFrameworkModule = true;
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics(IServiceResolver resolver)
        {
            var physics = resolver.Resolve<ISmellPhysicsConfiguration>();
            return (physics.CreateSmellPropagationAlgorithm(), physics.SmellPhysics);
        }
    }
}