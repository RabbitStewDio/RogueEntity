using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using System;

namespace RogueEntity.Core.Players
{
    public class PlayerModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Player";
        
        public static readonly EntityRole PlayerSpawnPointRole = new EntityRole("Role.Core.Player.PlayerSpawnPoint");
        public static readonly EntityRole PlayerRole = new EntityRole("Role.Core.Player");
        public static readonly EntityRole PlayerObserverRole = new EntityRole("Role.Core.Player.PlayerObserver");

        public static readonly EntityRelation PlayerToSpawnPointRelation = new EntityRelation("Relation.Core.Player.PlayerToSpawnPoint", PlayerRole, PlayerSpawnPointRole, true);
        
        public static readonly EntitySystemId PlayerComponentsId = "Entities.Core.Player";
        public static readonly EntitySystemId PlayerObserverComponentsId = "Entities.Core.PlayerObserver";
        public static readonly EntitySystemId PlayerSpawnPointComponentsId = "Entities.Core.PlayerSpawnPoint";
        public static readonly EntitySystemId RegisterPlayerServiceId = "Systems.Core.Player.RegisterPlayerService";
        public static readonly EntitySystemId RegisterPlayerObserverRefreshGridId = "Systems.Core.Player.RefreshObservers.Grid";
        public static readonly EntitySystemId RegisterPlayerObserverRefreshContinuousId = "Systems.Core.Player.RefreshObservers.Continuous";

        public PlayerModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Players and Observers";
            Description = "Provides base classes and behaviours for declaring player and observer entities ";
            IsFrameworkModule = true;
            
            DeclareDependency(ModuleDependency.Of(CoreModule.ModuleId));

            RequireRole(PlayerSpawnPointRole);
            RequireRole(PlayerObserverRole);
            RequireRole(PlayerRole).WithImpliedRole(CoreModule.EntityRole);

            RequireRelation(PlayerToSpawnPointRelation);
        }

        [EntityRoleInitializer("Role.Core.Player.PlayerSpawnPoint")]
        protected void InitializePlayerSpawnPointRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                     IModuleInitializer initializer,
                                                     EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(PlayerSpawnPointComponentsId, -20_000, RegisterPlayerSpawnPointComponents);
        }

        [EntityRoleInitializer("Role.Core.Player.PlayerObserver")]
        protected void InitializePlayerObserverRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                     IModuleInitializer initializer,
                                                     EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(PlayerObserverComponentsId, -20_000, RegisterPlayerObserverComponents);
        }

        [EntityRoleInitializer("Role.Core.Player")]
        protected void InitializePlayerRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                     IModuleInitializer initializer,
                                                     EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(PlayerComponentsId, -20_000, RegisterPlayerComponents);
            entityContext.Register(RegisterPlayerServiceId, -10_000, RegisterPlayerService);
            
        }

        void RegisterPlayerService<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            if (!TryGetOrCreatePlayerService(initParameter.ServiceResolver, registry, out var service))
            {
                throw new ModuleInitializationException("Require a player service to function");
            }
        }

        [EntityRoleInitializer("Role.Core.Player.PlayerObserver",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeRefreshPlayerObserversContinuous<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                           IModuleInitializer initializer,
                                                                           EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterPlayerObserverRefreshContinuousId, 80_000, RegisterRefreshObservers<TItemId, ContinuousMapPosition>);
        }

        [EntityRoleInitializer("Role.Core.Player.PlayerObserver",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeRefreshPlayerObserversGrid<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                     IModuleInitializer initializer,
                                                                     EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterPlayerObserverRefreshGridId, 80_000, RegisterRefreshObservers<TItemId, EntityGridPosition>);
        }

        void RegisterRefreshObservers<TItemId, TPosition>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                          IGameLoopSystemRegistration context,
                                                          EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
            where TPosition : IPosition<TPosition>

        {
            if (!TryGetOrCreatePlayerService(initParameter.ServiceResolver, registry, out var playerService))
            {
                // some one else has created a different service implementation.
                return;
            }

            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<PlayerObserver>()
                                 .WithInputParameter<TPosition>()
                                 .CreateSystem(playerService.RefreshObservers);

            context.AddInitializationStepHandler(action, nameof(playerService.RefreshObservers));
            context.AddLateFixedStepHandlers(action, nameof(playerService.RefreshObservers));
        }

        void RegisterPlayerComponents<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                               EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<PlayerTag>();
        }

        void RegisterPlayerObserverComponents<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                               EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<PlayerObserver>();
        }

        void RegisterPlayerSpawnPointComponents<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                               EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<PlayerSpawnLocation>();
        }

        bool TryGetOrCreatePlayerService<TItemId>(IServiceResolver r,
                                                  EntityRegistry<TItemId> registry,
                                                  out PlayerService<TItemId> ps)
            where TItemId : IEntityKey
        {
            if (r.TryResolve(out ps))
            {
                return true;
            }

            if (r.TryResolve(out IPlayerService<TItemId> _))
            {
                ps = default;
                return false;
            }

            var ir = r.Resolve<IItemResolver<TItemId>>();
            var conf = r.Resolve<IPlayerServiceConfiguration>();

            var playerDeclaration = ir.ItemRegistry.ReferenceItemById(conf.PlayerId);
            if (!playerDeclaration.HasItemComponent<TItemId, PlayerTag>())
            {
                throw new InvalidOperationException($"Player entity {conf.PlayerId} does not declare a PlayerTag trait");
            }

            ps = new PlayerService<TItemId>(registry.PersistentView<PlayerTag>());
            r.Store(ps);
            r.Store<IPlayerService<TItemId>>(ps);
            return true;
        }
    }
}
