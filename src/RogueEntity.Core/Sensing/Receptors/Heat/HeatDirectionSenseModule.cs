using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources.Heat;

namespace RogueEntity.Core.Sensing.Receptors.Heat
{
    [Module]
    public class HeatDirectionSenseModule : SenseReceptorModuleBase<TemperatureSense, TemperatureSense, HeatSourceDefinition>
    {
        public const string ModuleId = "Core.Sense.Receptor.Temperature";

        public HeatDirectionSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors - Heat";
            Description = "Provides items and actors with a field of view for a directional sense of Temperature.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(HeatSourceModule.ModuleId));
        }

        protected override void RegisterCalculateDirectionalSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                          IGameLoopSystemRegistration<TGameContext> context,
                                                                                          EntityRegistry<TItemId> registry)
        {
            RegisterCalculateUniDirectionalSystem(in initParameter, context, registry);
        }

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            var physics = serviceResolver.GetOrCreateHeatSensorPhysics();
            return (physics.CreateHeatSensorPropagationAlgorithm(), physics.HeatPhysics);
        }
    }
}