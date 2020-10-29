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

namespace RogueEntity.Core.Sensing.Sources.Light
{
    /// <summary>
    ///   Registers light source calculation entities.
    /// </summary>
    /// <remarks>
    ///   Defines the following systems:
    ///
    ///   5000 - preparation: Clear collected sources, fetch current time, etc.
    ///   5700 - collect all sense sources that are dirty. Specialized handling for grid and continuous positions.
    ///   5800 - recompute those collected sense sources.
    ///   5900 - clean up, mark all processed sources as clean.
    /// </remarks>
    public class LightSourceModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Senses.Source.Light";
        public static readonly EntityRole LightSourceRole = new EntityRole("Role.Core.Senses.Source.Light");

        public static readonly EntitySystemId LightsPreparationSystemId = "Systems.Core.Senses.Source.Light.Prepare";
        public static readonly EntitySystemId LightsCollectionGridSystemId = "Systems.Core.Senses.Source.Light.Collect.Grid";
        public static readonly EntitySystemId LightsCollectionContinuousSystemId = "Systems.Core.Senses.Source.Light.Collect.Continuous";
        public static readonly EntitySystemId LightsComputeSystemId = "Systems.Core.Senses.Source.Light.Compute";
        public static readonly EntitySystemId LightsFinializedSystemId = "Systems.Core.Senses.Source.Light.Finalize";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Source.Light";

        public LightSourceModule()
        {
            Id = ModuleId;

            DeclareDependencies(ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(LightSourceRole).WithImpliedRole(SenseSources.SenseSourceRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : ITimeContext
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(LightsPreparationSystemId, 50000, RegisterPrepareLightSystem);
            ctx.Register(LightsComputeSystemId, 58000, RegisterProcessLightCalculateSystem);
            ctx.Register(LightsFinializedSystemId, 59000, RegisterFinishLightCalculateSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeLightCollectionGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                            IModuleInitializer<TGameContext> initializer,
                                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(LightsCollectionGridSystemId, 55000, RegisterCollectLightsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeLightCollectionContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                  IModuleInitializer<TGameContext> initializer,
                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(LightsCollectionGridSystemId, 55000, RegisterCollectLightsContinuousSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light",
                               ConditionalRoles = new[]
                               {
                                   "Role.Core.Position.GridPositioned",
                                   "Role.Core.Senses.Cache.InvalidationSource"
                               })]
        protected void InitializeLightSenseCache<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IGridMapContext<TGameContext, TItemId>
        {
            if (serviceResolver.TryResolve(out SenseCacheSetUpSystem<TGameContext> o))
            {
                o.RegisterSense<VisionSense>();
            }
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

            var system = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<LightSourceDefinition, SenseSourceState<VisionSense>, ContinuousMapPosition>(ls.FindDirtySenseSources);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
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
                                 .CreateSystem<LightSourceDefinition, SenseSourceState<VisionSense>, EntityGridPosition>(ls.FindDirtySenseSources);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterProcessLightCalculateSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                        IGameLoopSystemRegistration<TGameContext> context,
                                                                        EntityRegistry<TItemId> registry,
                                                                        ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(resolver, out var ls))
            {
                return;
            }

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<LightSourceDefinition, SenseSourceState<VisionSense>, SenseDirtyFlag<VisionSense>, ObservedSenseSource<VisionSense>>(ls.RefreshLocalSenseState);

            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        void RegisterFinishLightCalculateSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
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
            context.AddDisposeStepHandler(ls.ShutDown);
        }

        static bool GetOrCreateLightSystem(IServiceResolver serviceResolver, out LightSystem ls)
        {
            if (!serviceResolver.TryResolve(out ILightPhysicsConfiguration physicsConfig))
            {
                ls = default;
                return false;
            }

            if (!serviceResolver.TryResolve(out ls))
            {
                ls = new LightSystem(serviceResolver.ResolveToReference<ISensePropertiesSource>(),
                                     serviceResolver.ResolveToReference<IGlobalSenseStateCacheProvider>(),
                                     serviceResolver.ResolveToReference<ITimeSource>(),
                                     serviceResolver.Resolve<ISenseStateCacheControl>(),
                                     physicsConfig.CreateLightPropagationAlgorithm(),
                                     physicsConfig);
            }

            return true;
        }

        void RegisterEntities<TItemId>(IServiceResolver serviceResolver, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<LightSourceDefinition>();
            registry.RegisterNonConstructable<SenseSourceState<VisionSense>>();
            registry.RegisterFlag<ObservedSenseSource<VisionSense>>();
            registry.RegisterFlag<SenseDirtyFlag<VisionSense>>();
        }
    }
}