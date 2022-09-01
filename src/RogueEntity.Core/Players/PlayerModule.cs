using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using Serilog;
using System;
using System.Linq;

namespace RogueEntity.Core.Players
{
    public class PlayerModule : ModuleBase
    {
        static readonly ILogger Logger = SLog.ForContext<PlayerModule>();
        
        public static readonly string ModuleId = "Core.Player";

        public static readonly EntityRole PlayerRole = new EntityRole("Role.Core.Player");
        public static readonly EntityRole PlayerObserverRole = new EntityRole("Role.Core.Player.PlayerObserver");

        public static readonly EntityRelation PlayerToObserverRelation = new EntityRelation("Relation.Core.Player.PlayerHasObservers", PlayerRole, PlayerObserverRole, true);

        public static readonly EntitySystemId PlayerComponentsId = "Entities.Core.Player";
        public static readonly EntitySystemId PlayerObserverComponentsId = "Entities.Core.PlayerObserver";
        public static readonly EntitySystemId PlayerSpawnPointComponentsId = "Entities.Core.PlayerSpawnPoint";
        public static readonly EntitySystemId RegisterPlayerServiceId = "System.Core.Player.RegisterPlayerService";
        public static readonly EntitySystemId RegisterPlayerObserverRefreshGridId = "System.Core.Player.RefreshObservers.Grid";
        public static readonly EntitySystemId RegisterPlayerObserverRefreshContinuousId = "System.Core.Player.RefreshObservers.Continuous";

        public PlayerModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Players and Observers";
            Description = "Provides base classes and behaviours for declaring player and observer entities ";
            IsFrameworkModule = true;

            DeclareDependency(ModuleDependency.Of(CoreModule.ModuleId));

            RequireRole(PlayerObserverRole);
            RequireRole(PlayerRole).WithImpliedRole(CoreModule.EntityRole);

            RequireRelation(PlayerToObserverRelation);
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
                                                     EntityRole role)
            where TItemId : IEntityKey
        {
            if (!initParameter.ServiceResolver.TryResolve(out IPlayerServiceConfiguration conf))
            {
                var itemResolver = initParameter.ServiceResolver.Resolve<IItemResolver<TItemId>>();

                var x = itemResolver.ItemRegistry.Items.Where(e => e.GetEntityRoles().Select(r => r.role.Role).Contains(PlayerRole)).Select(e => e.Id).ToList();
                if (x.Count > 1)
                {
                    Logger.Warning("Skipping auto-configuration of player configuration service as more than one item declares a player role trait: {ItemsDeclaringPlayerTrait}", x);
                }
                else if (x.Count == 1)
                {
                    conf = new PlayerServiceConfiguration(x[0]);
                    initParameter.ServiceResolver.Store(conf);
                    Logger.Information("Auto-configured player configuration service for player item {ItemDeclaringPlayerTrait}", x[0]);
                }
                else
                {
                    Logger.Warning("No item declared a player role trait; unable to provide automatic player configuration");
                }
            }

            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(PlayerComponentsId, -20_000, RegisterPlayerComponents);
            entityContext.Register(RegisterPlayerServiceId, 80_000, RegisterPlayerService);
        }

        [EntityRelationInitializer("Relation.Core.Player.PlayerHasObservers",
                                   ConditionalObjectRoles = new[] { "Role.Core.Position.ContinuousPositioned" })]
        protected void InitializeRefreshPlayerObserversContinuous<TActorId, TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                                     IModuleInitializer initializer,
                                                                                     EntityRelation r)
            where TItemId : IEntityKey
            where TActorId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterPlayerObserverRefreshContinuousId, 81_000, RegisterRefreshObservers<TActorId, TItemId, ContinuousMapPosition>);
        }

        [EntityRelationInitializer("Relation.Core.Player.PlayerHasObservers",
                                   ConditionalObjectRoles = new[] { "Role.Core.Position.GridPositioned" })]
        protected void InitializeRefreshPlayerObserversGrid<TActorId, TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                               IModuleInitializer initializer,
                                                                               EntityRelation r)
            where TItemId : IEntityKey
            where TActorId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterPlayerObserverRefreshGridId, 81_000, RegisterRefreshObservers<TActorId, TItemId, EntityGridPosition>);
        }

        void RegisterPlayerService<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                            IGameLoopSystemRegistration context,
                                            EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            if (!TryGetOrCreatePlayerService<TItemId>(initParameter.ServiceResolver, out var playerService))
            {
                throw new ModuleInitializationException("Require a player service to function");
            }

            if (!initParameter.ServiceResolver.TryResolve<IPlayerManager<TItemId>>(out var playerManager))
            {
                playerManager = new BasicPlayerManager<TItemId>(
                    initParameter.ServiceResolver.Resolve<IItemResolver<TItemId>>(),
                    initParameter.ServiceResolver.ResolveToReference<IPlayerServiceConfiguration>());
                initParameter.ServiceResolver.Store(playerManager);
            }

            var refreshPlayerSystem = registry.BuildSystem()
                                              .WithoutContext()
                                              .WithInputParameter<PlayerTag>()
                                              .CreateSystem(playerService.RefreshPlayers);

            context.AddInitializationStepHandlerSystem(refreshPlayerSystem);
            context.AddInitializationStepHandler(playerService.RemoveExpiredPlayerData);

            context.AddLateFixedStepHandlerSystem(refreshPlayerSystem);
            context.AddLateFixedStepHandlers(playerService.RemoveExpiredPlayerData);
        }

        void RegisterRefreshObservers<TActorId, TItemId, TPosition>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                    IGameLoopSystemRegistration context,
                                                                    EntityRegistry<TItemId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
            where TPosition : IPosition<TPosition>

        {
            if (!TryGetOrCreatePlayerService<TActorId>(initParameter.ServiceResolver, out var playerService))
            {
                // some one else has created a different service implementation.
                return;
            }

            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<PlayerObserverTag>()
                                 .WithInputParameter<TPosition>()
                                 .CreateSystem(playerService.RefreshObservers);

            context.AddInitializationStepHandlerSystem(action);
            context.AddInitializationStepHandler(playerService.RemoveObsoleteObservers);
            context.AddLateFixedStepHandlerSystem(action);
            context.AddLateFixedStepHandlers(playerService.RemoveObsoleteObservers);
        }

        void RegisterPlayerComponents<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                               EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<PlayerTag>();
            registry.RegisterFlag<NewPlayerSpawnRequest>();
        }

        void RegisterPlayerObserverComponents<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                       EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<PlayerObserverTag>();
        }

        bool TryGetOrCreatePlayerService<TItemId>(IServiceResolver r, out BasicPlayerService<TItemId> ps)
            where TItemId : IEntityKey
        {
            if (r.TryResolve(out ps))
            {
                return true;
            }

            if (r.TryResolve(out IPlayerService _))
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

            ps = new BasicPlayerService<TItemId>(r.ResolveToReference<ITimeSource>());
            r.Store(ps);
            r.Store<IPlayerService>(ps);
            r.Store<IPlayerLookup<TItemId>>(ps);
            return true;
        }
    }
}
