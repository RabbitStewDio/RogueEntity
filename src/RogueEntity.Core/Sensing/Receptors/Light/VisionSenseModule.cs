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
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Sensing.Receptors.Light
{
    public class VisionSenseModule : ModuleBase
    {
        public const string ModuleId = "Core.Sense.Receptor.Vision";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Receptor.Vision";

        public static readonly EntitySystemId ReceptorPreparationSystemId = "Systems.Core.Senses.Receptor.Vision.Prepare";
        public static readonly EntitySystemId ReceptorCollectionGridSystemId = "Systems.Core.Senses.Receptor.Vision.Collect.Grid";
        public static readonly EntitySystemId ReceptorCollectionContinuousSystemId = "Systems.Core.Senses.Receptor.Vision.Collect.Continuous";
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Receptor.Vision.Compute";

        public static readonly EntityRole SenseReceptorActorRole = new EntityRole("Role.Core.Senses.Receptor.Vision.ActorRole");

        public VisionSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId));

            RequireRole(SenseReceptorActorRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Vision.ActorRole")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : ITimeContext
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(ReceptorPreparationSystemId, 5000, RegisterPrepareLightSystem);
            ctx.Register(ReceptorComputeSystemId, 5900, RegisterLightCalculateSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Vision.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeLightCollectionGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                            IModuleInitializer<TGameContext> initializer,
                                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionGridSystemId, 5800, RegisterCollectLightsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Vision.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeLightCollectionContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                  IModuleInitializer<TGameContext> initializer,
                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionContinuousSystemId, 5800, RegisterCollectLightsContinuousSystem);
        }

        void RegisterPrepareLightSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                               IGameLoopSystemRegistration<TGameContext> context,
                                                               EntityRegistry<TItemId> registry,
                                                               ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
            where TGameContext : ITimeContext
        {
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable);
            context.AddInitializationStepHandler(ls.BeginSenseCalculation);
            context.AddFixedStepHandlers(ls.BeginSenseCalculation);
        }

        void RegisterCollectLightsGridSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                    EntityRegistry<TItemId> registry,
                                                                    ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(resolver, out var ls))
            {
                return;
            }

            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SensoryReceptorData<VisionSense>, SensoryReceptorState<VisionSense>, ContinuousMapPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<VisionSense>, SensoryReceptorState<VisionSense>, SenseReceptorDirtyFlag<VisionSense>, ContinuousMapPosition>(ls.RefreshLocalSenseState);
            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        void RegisterCollectLightsContinuousSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                          IGameLoopSystemRegistration<TGameContext> context,
                                                                          EntityRegistry<TItemId> registry,
                                                                          ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(resolver, out var ls))
            {
                return;
            }

            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SensoryReceptorData<VisionSense>, SensoryReceptorState<VisionSense>, EntityGridPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<VisionSense>, SensoryReceptorState<VisionSense>, SenseReceptorDirtyFlag<VisionSense>, EntityGridPosition>(ls.RefreshLocalSenseState);

            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        void RegisterLightCalculateSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                 IGameLoopSystemRegistration<TGameContext> context,
                                                                 EntityRegistry<TItemId> registry,
                                                                 ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

            context.AddInitializationStepHandler(ls.EndSenseCalculation);
            context.AddFixedStepHandlers(ls.EndSenseCalculation);
        }

        static bool GetOrCreateLightSystem(IServiceResolver serviceResolver, out VisionSenseSystem<VisionSense> ls)
        {
            if (!serviceResolver.TryResolve(out ILightPhysicsConfiguration physicsConfig))
            {
                ls = default;
                return false;
            }

            if (!serviceResolver.TryResolve(out ls))
            {
                ls = new VisionSenseSystem<VisionSense>(serviceResolver.ResolveToReference<ISensePropertiesSource>(),
                                                        serviceResolver.ResolveToReference<ISenseStateCacheProvider>(),
                                                        physicsConfig.LightPhysics,
                                                        physicsConfig.CreateLightPropagationAlgorithm());
            }

            return true;
        }

        void RegisterEntities<TItemId>(IServiceResolver serviceResolver, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.Register<SenseReceptorDirtyFlag<VisionSense>>();
            registry.RegisterNonConstructable<SensoryReceptorData<VisionSense>>();
            registry.RegisterNonConstructable<SensoryReceptorState<VisionSense>>();
        }
    }
}