using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
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
        public static readonly EntityRole PlayerRole = new EntityRole("Role.Core.Player");
        public static readonly EntityRole PlayerObserverRole = new EntityRole("Role.Core.PlayerObserver");
        public static readonly EntitySystemId PlayerComponentsId = "Entities.Core.Player";

        public static readonly EntitySystemId RegisterPlayerObserverRefresh = "Systems.Core.Player.RefreshObservers";

        public PlayerModule()
        {
            Id = "Core.Player";
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Equipment";
            Description = "Provides base classes and behaviours for equipping items";
            IsFrameworkModule = true;
            
            DeclareDependency(ModuleDependency.Of(CoreModule.ModuleId));

            RequireRole(PlayerRole).WithImpliedRole(CoreModule.EntityRole);
        }

        [EntityRoleInitializer("Role.Core.Player")]
        protected void InitializePlayerRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                     IModuleInitializer initializer,
                                                     EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(PlayerComponentsId, -20_000, RegisterPlayerComponents);
        }

        [EntityRoleInitializer("Role.Core.Player",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeRefreshPlayerObserversContinuous<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                           IModuleInitializer initializer,
                                                                           EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterPlayerObserverRefresh, 80_000, RegisterRefreshObservers<TItemId, ContinuousMapPosition>);
        }

        [EntityRoleInitializer("Role.Core.Player",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeRefreshPlayerObserversGrid<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                     IModuleInitializer initializer,
                                                                     EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterPlayerObserverRefresh, 80_000, RegisterRefreshObservers<TItemId, EntityGridPosition>);
        }

        void RegisterRefreshObservers<TItemId, TPosition>(in ModuleInitializationParameter initParameter,
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

        void RegisterPlayerComponents<TItemId>(in ModuleInitializationParameter initParameter,
                                               EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<PlayerTag>();
            registry.RegisterNonConstructable<PlayerObserver>();
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

            if (r.TryResolve(out IPlayerService<TItemId> otherService))
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

            ps = new PlayerService<TItemId>(ir,
                                            registry.PersistentView<PlayerTag>(),
                                            registry.PersistentView<PlayerObserver>(),
                                            conf.PlayerId);
            r.Store(ps);
            r.Store<IPlayerService<TItemId>>(ps);
            return true;
        }
    }
}
