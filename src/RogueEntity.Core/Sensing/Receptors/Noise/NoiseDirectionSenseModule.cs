using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Sources.Noise;

namespace RogueEntity.Core.Sensing.Receptors.Noise
{
    public class NoiseDirectionSenseModule : SenseReceptorModuleBase<NoiseSense, NoiseSense, NoiseSourceDefinition>
    {
        public const string ModuleId = "Core.Sense.Receptor.Noise";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Receptor.Noise";

        public static readonly EntitySystemId ReceptorPreparationSystemId = "Systems.Core.Senses.Receptor.Noise.Prepare";
        public static readonly EntitySystemId ReceptorCollectionGridSystemId = "Systems.Core.Senses.Receptor.Noise.Collect.Grid";
        public static readonly EntitySystemId ReceptorCollectionContinuousSystemId = "Systems.Core.Senses.Receptor.Noise.Collect.Continuous";
        public static readonly EntitySystemId SenseSourceCollectionContinuousSystemId = "Systems.Core.Senses.Receptor.Noise.CollectSources.Continuous";
        public static readonly EntitySystemId ReceptorComputeFoVSystemId = "Systems.Core.Senses.Receptor.Noise.ComputeFieldOfView";
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Receptor.Noise.Compute";
        public static readonly EntitySystemId ReceptorFinalizeSystemId = "Systems.Core.Senses.Receptor.Noise.Finalize";

        public static readonly EntityRole SenseReceptorActorRole = new EntityRole("Role.Core.Senses.Receptor.Noise.ActorRole");

        public NoiseDirectionSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view for a directional sense of noise.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(NoiseSourceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId));

            RequireRole(SenseReceptorActorRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Noise.ActorRole")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(ReceptorPreparationSystemId, 50000, RegisterPrepareSystem);
            ctx.Register(ReceptorComputeFoVSystemId, 56000, RegisterComputeReceptorFieldOfView);
            ctx.Register(ReceptorComputeSystemId, 58500, RegisterCalculateUniDirectionalSystem);
            ctx.Register(ReceptorFinalizeSystemId, 59500, RegisterFinalizeSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Noise.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeCollectReceptorsGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                             IModuleInitializer<TGameContext> initializer,
                                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionGridSystemId, 55500, RegisterCollectReceptorsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Noise.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeCollectReceptorsContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                   IModuleInitializer<TGameContext> initializer,
                                                                                   EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionContinuousSystemId, 55500, RegisterCollectReceptorsContinuousSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Noise")]
        protected void InitializeSenseCollection<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseSourceCollectionContinuousSystemId, 57500, RegisterCollectSenseSourcesSystem);
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