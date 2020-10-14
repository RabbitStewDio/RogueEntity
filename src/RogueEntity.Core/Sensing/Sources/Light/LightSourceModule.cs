using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public class LightSourceModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Senses.Source.Light";
        public static readonly EntityRole LightSourceRole = new EntityRole("Role.Core.Senses.Source.Light");

        public static readonly EntitySystemId LightsPreparationSystemId = "Systems.Core.Senses.Source.Light.Prepare";
        public static readonly EntitySystemId LightsCollectionGridSystemId = "Systems.Core.Senses.Source.Light.Collect.Grid";
        public static readonly EntitySystemId LightsCollectionContinuousSystemId = "Systems.Core.Senses.Source.Light.Collect.Continuous";
        public static readonly EntitySystemId LightsComputeSystemId = "Systems.Core.Senses.Source.Light.Compute";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Source.Light";

        public LightSourceModule()
        {
            Id = ModuleId;

            DeclareDependencies(ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(LightSourceRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(LightsPreparationSystemId, 5000, RegisterPrepareLightSystem);
            ctx.Register(LightsComputeSystemId, 5900, RegisterLightCalculateSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeLightCollectionGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                            IModuleInitializer<TGameContext> initializer,
                                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(LightsCollectionGridSystemId, 5800, RegisterCollectLightsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeLightCollectionContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                  IModuleInitializer<TGameContext> initializer,
                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(LightsCollectionGridSystemId, 5800, RegisterCollectLightsContinuousSystem);
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

            var system = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<LightSourceDefinition, SenseSourceState<VisionSense>, ContinuousMapPosition>(ls.CollectLights);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<LightSourceDefinition, SenseSourceState<VisionSense>, SenseDirtyFlag<VisionSense>, ContinuousMapPosition>(ls.RefreshLocalSenseState);
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
                                 .CreateSystem<LightSourceDefinition, SenseSourceState<VisionSense>, EntityGridPosition>(ls.CollectLights);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<LightSourceDefinition, SenseSourceState<VisionSense>, SenseDirtyFlag<VisionSense>, EntityGridPosition>(ls.RefreshLocalSenseState);
            
            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        void RegisterPrepareLightSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                               IGameLoopSystemRegistration<TGameContext> context,
                                                               EntityRegistry<TItemId> registry,
                                                               ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable);
            context.AddInitializationStepHandler(ls.BeginSenseCalculation);
            context.AddFixedStepHandlers(ls.BeginSenseCalculation);
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


            context.AddInitializationStepHandler(ctx => ls.ProcessSenseMap(registry));
            context.AddFixedStepHandlers(ctx => ls.ProcessSenseMap(registry));
            
            context.AddInitializationStepHandler(ls.EndSenseCalculation);
            context.AddFixedStepHandlers(ls.EndSenseCalculation);
        }

        static bool GetOrCreateLightSystem(IServiceResolver serviceResolver, out LightSystem ls)
        {
            if (!serviceResolver.TryResolve(out ISenseDataBlitter senseBlitterFactory) ||
                !serviceResolver.TryResolve(out ILightPhysicsConfiguration physicsConfig))
            {
                ls = default;
                return false;
            }

            if (!serviceResolver.TryResolve(out ls))
            {
                ls = new LightSystem(serviceResolver.ResolveToReference<ISensePropertiesSource>(),
                                     serviceResolver.ResolveToReference<ISenseStateCacheProvider>(),
                                     new ShadowPropagationAlgorithm(physicsConfig.LightPhysics),
                                     senseBlitterFactory);
            }

            return true;
        }

        void RegisterEntities<TItemId>(IServiceResolver serviceResolver, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<LightSourceDefinition>();
            registry.RegisterNonConstructable<SenseSourceState<VisionSense>>();
        }
    }
}