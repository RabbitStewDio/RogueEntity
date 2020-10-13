using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Sensing.Receptors
{
    [Module]
    public class SensoryReceptorModule : ModuleBase
    {
        public const string ModuleId = "Core.Sense.Receptor";

        public static readonly EntitySystemId RegisterEntities = "Entities.Core.Senses.Receptor";
        public static readonly EntitySystemId RegisterSystem = "Systems.Core.Senses.Receptor";
        public static readonly EntitySystemId RegisterCleanupSystem = "Systems.Core.Senses.Receptor.MarkClean";

        public static readonly EntityRole SenseReceptorActorRole = new EntityRole("Role.Core.Senses.Receptor.ActorRole");
        public static readonly EntityRole SenseReceptorActorRoleGrid = new EntityRole("Role.Core.Senses.Receptor.ActorRole.Grid");
        public static readonly EntityRole SenseReceptorActorRoleContinuous = new EntityRole("Role.Core.Senses.Receptor.ActorRole.Continuous");

        public SensoryReceptorModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view.";
            IsFrameworkModule = true;

            // todo
            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId));

            RequireRole(SenseReceptorActorRole);
            RequireRole(SenseReceptorActorRoleContinuous).WithImpliedRole(SenseReceptorActorRole);
            RequireRole(SenseReceptorActorRoleGrid).WithImpliedRole(SenseReceptorActorRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.ActorRole")]
        protected void Initialize<TGameContext, TActorId>(IServiceResolver serviceResolver,
                                                          IModuleInitializer<TGameContext> initializer,
                                                          EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterEntities, 0, RegisterEntityComponents);
            ctx.Register(RegisterCleanupSystem, 0, RegisterMarkEntitiesCleanSystem);
        }

        void RegisterMarkEntitiesCleanSystem<TGameContext, TActorId>(IServiceResolver serviceResolver,
                                                                     IGameLoopSystemRegistration<TGameContext> context,
                                                                     EntityRegistry<TActorId> registry,
                                                                     ICommandHandlerRegistration<TGameContext, TActorId> handler)
            where TActorId : IEntityKey
        {
            var s = GetOrCreateVisionSystem(serviceResolver);
            var system = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<SensoryReceptor<VisionSense>>(s.MarkVisionClean);

            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterEntityComponents<TActorId>(IServiceResolver serviceResolver,
                                                EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<VisionSense>();
            registry.RegisterNonConstructable<SensoryReceptor<VisionSense>>();
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.ActorRole.Grid")]
        protected void InitializeGrid<TGameContext, TActorId>(IServiceResolver serviceResolver,
                                                              IModuleInitializer<TGameContext> initializer,
                                                              EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterSystem, 5900, RegisterUpdateVisionAreaSystemGrid);
        }

        void RegisterUpdateVisionAreaSystemGrid<TGameContext, TActorId>(IServiceResolver resolver,
                                                                        IGameLoopSystemRegistration<TGameContext> context,
                                                                        EntityRegistry<TActorId> registry,
                                                                        ICommandHandlerRegistration<TGameContext, TActorId> handler)
            where TActorId : IEntityKey
        {
            var s = GetOrCreateVisionSystem(resolver);

            var system = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<EntityGridPosition, VisionSense, SensoryReceptor<VisionSense>>(s.UpdateLocalVisionGrid);

            context.AddFixedStepHandlers(system);
            context.AddInitializationStepHandler(system);
        }

        VisionSenseSystem GetOrCreateVisionSystem(IServiceResolver resolver)
        {
            if (resolver.TryResolve(out VisionSenseSystem s))
            {
                return s;
            }

            s = new VisionSenseSystem(resolver.ResolveToReference<ISensePropertiesSource>(),
                                      resolver.ResolveToReference<ISenseStateCacheProvider>());
            resolver.Store(s);
            return s;
        }
    }
}