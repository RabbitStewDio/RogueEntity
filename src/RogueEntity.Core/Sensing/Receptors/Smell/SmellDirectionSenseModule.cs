using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources.Smell;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    [Module]
    public class SmellDirectionSenseModule : SenseReceptorModuleBase<SmellSense, SmellSense, SmellSourceDefinition>
    {
        public const string ModuleId = "Core.Sense.Receptor.Smell";

        public SmellDirectionSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view for a directional sense of Smell.";
            IsFrameworkModule = true;

            DeclareDependency(ModuleDependency.Of(SmellSourceModule.ModuleId));

            RequireRole(SenseReceptorActorRole);
        }

        protected override void RegisterCalculateDirectionalSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                                                          IGameLoopSystemRegistration context,
                                                                                          EntityRegistry<TItemId> registry)
        {
            RegisterCalculateUniDirectionalSystem(in initParameter, context, registry);
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            var physics = serviceResolver.GetOrCreateSmellSensorPhysics();
            return (physics.CreateSmellSensorPropagationAlgorithm(), physics.SmellPhysics);
        }
    }
}