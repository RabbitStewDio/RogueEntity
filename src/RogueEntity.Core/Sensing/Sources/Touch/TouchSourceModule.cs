using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;

namespace RogueEntity.Core.Sensing.Sources.Touch
{
    [Module]
    public class TouchSourceModule : SenseSourceModuleBase<TouchSense, TouchSourceDefinition>
    {
        public static readonly string ModuleId = "Core.Senses.Source.Touch";

        public TouchSourceModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - SenseSource - Touch";
            Description = "Provides sense sources and sense resistance for touch";
            IsFrameworkModule = true;
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics(IServiceResolver serviceResolver)
        {
            var physics = serviceResolver.GetOrCreateTouchPhysics();
            return (physics.CreateTouchSensorPropagationAlgorithm(), physics.TouchPhysics);
        }

    }
}