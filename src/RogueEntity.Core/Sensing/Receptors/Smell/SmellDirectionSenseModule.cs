using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Common.FloodFill;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Smell;

namespace RogueEntity.Core.Sensing.Receptors.Smell
{
    public class SmellDirectionSenseModule : SenseReceptorModuleBase<SmellSense, SmellSense, SmellSourceDefinition>
    {
        public const string ModuleId = "Core.Sense.Receptor.Smell";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Receptor.Smell";

        public static readonly EntitySystemId ReceptorPreparationSystemId = "Systems.Core.Senses.Receptor.Smell.Prepare";
        public static readonly EntitySystemId ReceptorCollectionGridSystemId = "Systems.Core.Senses.Receptor.Smell.Collect.Grid";
        public static readonly EntitySystemId ReceptorCollectionContinuousSystemId = "Systems.Core.Senses.Receptor.Smell.Collect.Continuous";
        public static readonly EntitySystemId SenseSourceCollectionContinuousSystemId = "Systems.Core.Senses.Receptor.Smell.CollectSources.Continuous";
        public static readonly EntitySystemId ReceptorComputeFoVSystemId = "Systems.Core.Senses.Receptor.Smell.ComputeFieldOfView";
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Receptor.Smell.Compute";
        public static readonly EntitySystemId ReceptorFinalizeSystemId = "Systems.Core.Senses.Receptor.Smell.Finalize";

        public static readonly EntityRole SenseReceptorActorRole = new EntityRole("Role.Core.Senses.Receptor.Smell.ActorRole");

        public SmellDirectionSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view for a directional sense of Smell.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(SmellSourceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId));

            RequireRole(SenseReceptorActorRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Smell.ActorRole")]
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

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Smell.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeCollectReceptorsGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                             IModuleInitializer<TGameContext> initializer,
                                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionGridSystemId, 55500, RegisterCollectReceptorsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Smell.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeCollectReceptorsContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                   IModuleInitializer<TGameContext> initializer,
                                                                                   EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionContinuousSystemId, 55500, RegisterCollectReceptorsContinuousSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Smell")]
        protected void InitializeSenseCollection<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseSourceCollectionContinuousSystemId, 57500, RegisterCollectSenseSourcesSystem);
        }

        protected override bool GetOrCreateLightSystem(IServiceResolver serviceResolver, out SenseReceptorSystem<SmellSense, SmellSense> ls)
        {
            if (!serviceResolver.TryResolve(out ISmellSenseReceptorPhysicsConfiguration physics))
            {
                if (!serviceResolver.TryResolve(out ISmellPhysicsConfiguration physicsConfig))
                {
                    ls = default;
                    return false;
                }

                if (!serviceResolver.TryResolve(out FloodFillWorkingDataSource ds))
                {
                    ds = new FloodFillWorkingDataSource();
                }
                
                physics = new SmellSenseReceptorPhysicsConfiguration(physicsConfig, ds);
            }

            if (!serviceResolver.TryResolve(out ls))
            {
                ls = new SmellReceptorSystem(serviceResolver.ResolveToReference<ISensePropertiesSource>(),
                                             serviceResolver.ResolveToReference<ISenseStateCacheProvider>(),
                                             serviceResolver.ResolveToReference<IGlobalSenseStateCacheProvider>(),
                                             serviceResolver.ResolveToReference<ITimeSource>(),
                                             physics.SmellPhysics,
                                             physics.CreateSmellSensorPropagationAlgorithm());
            }

            return true;
        }
    }
}