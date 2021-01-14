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
            DeclareDependency(ModuleDependency.Of(CoreModule.ModuleId));

            RequireRole(PlayerRole).WithImpliedRole(CoreModule.EntityRole);
        }

        [EntityRoleInitializer("Role.Core.Player")]
        protected void InitializePlayerRole<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                   IModuleInitializer<TGameContext> initializer,
                                                                   EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(PlayerComponentsId, -20_000, RegisterPlayerComponents);
        }

        [EntityRoleInitializer("Role.Core.Player",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeRefreshPlayerObserversContinuous<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                                         IModuleInitializer<TGameContext> initializer,
                                                                                         EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterPlayerObserverRefresh, 80_000, RegisterRefreshObservers<TGameContext, TItemId, ContinuousMapPosition>);
        }

        [EntityRoleInitializer("Role.Core.Player",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeRefreshPlayerObserversGrid<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                                   IModuleInitializer<TGameContext> initializer,
                                                                                   EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterPlayerObserverRefresh, 80_000, RegisterRefreshObservers<TGameContext, TItemId, EntityGridPosition>);
        }

        void RegisterRefreshObservers<TGameContext, TItemId, TPosition>(in ModuleInitializationParameter initParameter,
                                                                        IGameLoopSystemRegistration<TGameContext> context,
                                                                        EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
            where TPosition : IPosition<TPosition>

        {
            if (!TryGetOrCreatePlayerService<TGameContext, TItemId>(initParameter.ServiceResolver, registry, out var playerService))
            {
                // some one else has created a different service implementation.
                return;
            }

            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<PlayerObserver>()
                                 .WithInputParameter<TPosition>()
                                 .CreateSystem(playerService.RefreshObservers);

            context.AddInitializationStepHandler(c => action(), nameof(playerService.RefreshObservers));
            context.AddLateFixedStepHandlers(c => action(), nameof(playerService.RefreshObservers));
        }

        void RegisterPlayerComponents<TItemId>(in ModuleInitializationParameter initParameter,
                                               EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<PlayerTag>();
            registry.RegisterNonConstructable<PlayerObserver>();
        }

        bool TryGetOrCreatePlayerService<TGameContext, TItemId>(IServiceResolver r,
                                                                EntityRegistry<TItemId> registry,
                                                                out PlayerService<TGameContext, TItemId> ps)
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

            var ir = r.Resolve<IItemResolver<TGameContext, TItemId>>();
            var conf = r.Resolve<IPlayerServiceConfiguration>();

            var playerDeclaration = ir.ItemRegistry.ReferenceItemById(conf.PlayerId);
            if (!playerDeclaration.HasItemComponent<TGameContext, TItemId, PlayerTag>())
            {
                throw new InvalidOperationException($"Player entity {conf.PlayerId} does not declare a PlayerTag trait");
            }

            ps = new PlayerService<TGameContext, TItemId>(ir,
                                                          registry.PersistentView<PlayerTag>(),
                                                          registry.PersistentView<PlayerObserver>(),
                                                          conf.PlayerId);
            return true;
        }
    }
}
