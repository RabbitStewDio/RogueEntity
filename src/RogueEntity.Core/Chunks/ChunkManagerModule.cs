using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Chunks
{
    public class ChunkManagerModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Chunks";

        public static readonly EntityRole ChunkLoadingObserverRole = new EntityRole("Role.Core.Chunks.Observer");

        public static readonly EntitySystemId ProcessGridPositionedObserversSystem = new EntitySystemId("System.Core.Chunks.ProcessObservers.GridPositioned");
        public static readonly EntitySystemId ProcessContinuousPositionedObserversSystem = new EntitySystemId("System.Core.Chunks.ProcessObservers.ContinuousPositioned");
        public static readonly EntitySystemId ProcessBeginMarkPhaseSystem = new EntitySystemId("System.Core.Chunks.BeginMarkPhase");
        public static readonly EntitySystemId ProcessLoadChunksSystem = new EntitySystemId("System.Core.Chunks.LoadChunks");
        public static readonly EntitySystemId ProcessUnloadChunksSystem = new EntitySystemId("System.Core.Chunks.UnloadChunks");

        public ChunkManagerModule()
        {
            Id = ModuleId;

            DeclareDependency(ModuleDependency.Of(PositionModule.ModuleId));
            DeclareDependency(ModuleDependency.Of(PlayerModule.ModuleId));

            RequireRole(ChunkLoadingObserverRole);
        }

        [EntityRoleInitializer("Role.Core.Chunks.Observer",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeGridPositionedPlayerObserverRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                           IModuleInitializer initializer,
                                                                           EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(ProcessGridPositionedObserversSystem, 47_000, RegisterProcessGridObserverPhase<TItemId, EntityGridPosition>);
        }

        [EntityRoleInitializer("Role.Core.Chunks.Observer",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeContinuousPositionedPlayerObserverRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                                 IModuleInitializer initializer,
                                                                                 EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(ProcessContinuousPositionedObserversSystem, 47_000, RegisterProcessGridObserverPhase<TItemId, ContinuousMapPosition>);
        }

        [ModuleInitializer]
        protected void InitializePlayerObserverRole(in ModuleInitializationParameter initParameter,
                                                    IModuleInitializer moduleInitializer)
        {
            if (!initParameter.EntityInformation.HasRole(ChunkLoadingObserverRole))
            {
                return;
            }

            moduleInitializer.Register(ProcessBeginMarkPhaseSystem, 100, RegisterBeginMarkPhaseSystem);
            moduleInitializer.Register(ProcessLoadChunksSystem, 48_500, RegisterLoadChunksSystem);
            moduleInitializer.Register(ProcessUnloadChunksSystem, 48_000, RegisterUnloadChunksSystem);
        }

        void RegisterLoadChunksSystem(in ModuleInitializationParameter initParameter, IGameLoopSystemRegistration context)
        {
            if (initParameter.ServiceResolver.TryResolve(out IMapLevelDataSourceSystem system))
            {
                context.AddInitializationStepHandler(system.LoadChunks);

                context.AddLateFixedStepHandlers(system.LoadChunks);
                context.AddLateVariableStepHandlers(system.LoadChunks);
            }
        }

        void RegisterUnloadChunksSystem(in ModuleInitializationParameter initParameter, IGameLoopSystemRegistration context)
        {
            if (initParameter.ServiceResolver.TryResolve(out IMapLevelDataSourceSystem system))
            {
                context.AddInitializationStepHandler(system.WriteChunks);
                context.AddInitializationStepHandler(system.UnloadChunks);

                context.AddLateVariableStepHandlers(system.WriteChunks);
                context.AddLateVariableStepHandlers(system.UnloadChunks);
            }
        }

        void RegisterBeginMarkPhaseSystem(in ModuleInitializationParameter initParameter, IGameLoopSystemRegistration context)
        {
            var r = GetOrCreateChunkManager(initParameter.ServiceResolver);

            context.AddInitializationStepHandler(r.Activate);
            context.AddDisposeStepHandler(r.Deactivate);

            context.AddInitializationStepHandler(r.BeginMarkPhase);
            context.AddLateVariableStepHandlers(r.BeginMarkPhase);
        }

        void RegisterProcessGridObserverPhase<TItemId, TPosition>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                  IGameLoopSystemRegistration context,
                                                                  EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
            where TPosition : IPosition<TPosition>
        {
            var r = GetOrCreateChunkManager(initParameter.ServiceResolver);

            var processObserversSystem = registry.BuildSystem()
                                                 .WithoutContext()
                                                 .WithInputParameter<PlayerObserver>()
                                                 .WithInputParameter<TPosition>()
                                                 .CreateSystem(r.ProcessObservers);

            context.AddInitializationStepHandler(processObserversSystem);
            context.AddInitializationStepHandler(r.FinalizeMarkPhase);

            context.AddLateVariableStepHandlers(processObserversSystem);
            context.AddLateVariableStepHandlers(r.FinalizeMarkPhase);
        }

        FullLevelChunkManager GetOrCreateChunkManager(IServiceResolver resolver)
        {
            if (resolver.TryResolve(out FullLevelChunkManager mgr))
            {
                return mgr;
            }

            mgr = new FullLevelChunkManager(resolver.ResolveToReference<ITimeSource>(),
                                            resolver.ResolveToReference<IMapLevelDataSource<int>>(), 1);
            resolver.Store(mgr);
            return mgr;
        }
    }
}
