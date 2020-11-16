using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
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
        
        protected override void RegisterCalculateDirectionalSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                          IGameLoopSystemRegistration<TGameContext> context,
                                                                                          EntityRegistry<TItemId> registry)
        {
            RegisterCalculateUniDirectionalSystem(in initParameter, context, registry);
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            var physics = serviceResolver.GetOrCreateTouchPhysics();
            return (physics.CreateTouchSensorPropagationAlgorithm(), physics.TouchPhysics);
        }
    }
}