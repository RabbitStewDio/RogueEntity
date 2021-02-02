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
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Chunks
{
    public class ChunkManagerModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Chunks";

        public static readonly EntitySystemId ProcessGridPositionedObserversSystem = new EntitySystemId("System.Core.Chunks.ProcessObservers.GridPositioned");
        public static readonly EntitySystemId ProcessBeginMarkPhaseSystem = new EntitySystemId("System.Core.Chunks.BeginMarkPhase");
        public static readonly EntitySystemId ProcessLoadChunksSystem = new EntitySystemId("System.Core.Chunks.LoadChunks");
        public static readonly EntitySystemId ProcessUnloadChunksSystem = new EntitySystemId("System.Core.Chunks.UnloadChunks");

        public ChunkManagerModule()
        {
            Id = ModuleId;

            DeclareDependency(ModuleDependency.Of(PositionModule.ModuleId));
            DeclareDependency(ModuleDependency.Of(PlayerModule.ModuleId));

            RequireRole(PositionModule.GridPositionedRole);
            RequireRole(PlayerModule.PlayerObserverRole);
        }

        [EntityRoleInitializer("Role.Core.Player.PlayerObserver", 
                               ConditionalRoles = new []{"Role.Core.Position.GridPositioned"})]
        protected void InitializeGridPositionedPlayerObserverRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                            IModuleInitializer initializer,
                                                            EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(ProcessGridPositionedObserversSystem, 47_000, RegisterProcessGridObserverPhase);
        }

        [ModuleInitializer]
        protected void InitializePlayerObserverRole(in ModuleInitializationParameter initParameter,
                                                             IModuleInitializer moduleInitializer)
        {
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

        void RegisterProcessGridObserverPhase<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var r = GetOrCreateChunkManager(initParameter.ServiceResolver);

            var processObserversSystem = registry.BuildSystem()
                                                 .WithoutContext()
                                                 .WithInputParameter<PlayerObserver>()
                                                 .WithInputParameter<EntityGridPosition>()
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
