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

        [EntityRoleInitializer("Role.Core.Position.GridPositioned")]
        protected void InitializeContainedItemRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                            IModuleInitializer initializer,
                                                            EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(ProcessBeginMarkPhaseSystem, 100, RegisterBeginMarkPhaseSystem);
            entityContext.Register(ProcessGridPositionedObserversSystem, 47_000, RegisterProcessGridObserverPhase);
            entityContext.Register(ProcessLoadChunksSystem, 48_500, RegisterLoadChunksSystem);
            entityContext.Register(ProcessUnloadChunksSystem, 48_000, RegisterUnloadChunksSystem);
        }

        void RegisterLoadChunksSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            if (initParameter.ServiceResolver.TryResolve(out IMapLevelDataSourceSystem system))
            {
                context.AddInitializationStepHandler(system.LoadChunks);
            }
        }

        void RegisterUnloadChunksSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            if (initParameter.ServiceResolver.TryResolve(out IMapLevelDataSourceSystem system))
            {
                context.AddInitializationStepHandler(system.UnloadChunks);
            }
        }

        void RegisterBeginMarkPhaseSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var r = GetOrCreateChunkManager(initParameter.ServiceResolver);
            
            context.AddInitializationStepHandler(r.Activate);
            context.AddDisposeStepHandler(r.Deactivate);

            context.AddInitializationStepHandler(r.BeginMarkPhase);
            context.AddFixedStepHandlers(r.BeginMarkPhase);
        }
        
        void RegisterProcessGridObserverPhase<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var r = GetOrCreateChunkManager(initParameter.ServiceResolver);
            
            var processObserversSystem = registry.BuildSystem().WithoutContext().WithInputParameter<PlayerObserver>().WithInputParameter<EntityGridPosition>().CreateSystem(r.ProcessObservers);

            context.AddInitializationStepHandler(processObserversSystem);
            context.AddInitializationStepHandler(r.FinalizeMarkPhase);

            context.AddFixedStepHandlers(processObserversSystem);
            context.AddFixedStepHandlers(r.FinalizeMarkPhase);
        }

        FullLevelChunkManager GetOrCreateChunkManager(IServiceResolver resolver)
        {
            if (resolver.TryResolve(out FullLevelChunkManager mgr))
            {
                return mgr;
            }

            mgr = new FullLevelChunkManager(resolver.ResolveToReference<ITimeSource>(),
                                            resolver.ResolveToReference<IMapLevelDataSource>(), 1);
            resolver.Store(mgr);
            return mgr;
        }
    }
}
