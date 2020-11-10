using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
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

        protected override (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out ISmellSenseReceptorPhysicsConfiguration physics))
            {
                var physicsConfig = serviceResolver.Resolve<ISmellPhysicsConfiguration>();
                if (!serviceResolver.TryResolve(out FloodFillWorkingDataSource ds))
                {
                    ds = new FloodFillWorkingDataSource();
                }

                physics = new SmellSenseReceptorPhysicsConfiguration(physicsConfig, ds);
            }

            return (physics.CreateSmellSensorPropagationAlgorithm(), physics.SmellPhysics);
        }
    }
}