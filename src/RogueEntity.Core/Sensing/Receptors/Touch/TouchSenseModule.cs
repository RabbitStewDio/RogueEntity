using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources.Touch;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    [Module]
    public class TouchSenseModule : SenseReceptorModuleBase<TouchSense, TouchSense, TouchSourceDefinition>
    {
        public const string ModuleId = "Core.Sense.Receptor.Touch";

        public TouchSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view for a omnidirectional sense of Touch.";
            IsFrameworkModule = true;

            DeclareDependency(ModuleDependency.Of(TouchSourceModule.ModuleId));

            RequireRole(SenseReceptorActorRole).WithImpliedRole(TouchSourceModule.SenseSourceRole);
        }
        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            var physics = serviceResolver.Resolve<ITouchReceptorPhysicsConfiguration>();
            return (physics.CreateTouchSensorPropagationAlgorithm(), physics.TouchPhysics);
        }
    }
}