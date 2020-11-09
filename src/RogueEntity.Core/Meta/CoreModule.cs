using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Meta
{
    [Module]
    public class CoreModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.MetaModule";

        public static readonly EntitySystemId CommonComponentsId = "Entities.Core.CommonEntities";
        public static readonly EntitySystemId PlayerComponentsId = "Entities.Core.Player";
        public static readonly EntitySystemId WorldItemComponentsId = "Entities.Core.Item";
        public static readonly EntitySystemId ContainedComponentsId = "Entities.Core.ContainedItem";

        public static readonly EntitySystemId DestroyItemsSystemId = "Core.Entity.DestroyMarked";
        public static readonly EntitySystemId DestroyCascadingItemsSystemId = "Core.Entity.DestroyCascadingMarked";

        public static readonly EntityRole PlayerRole = new EntityRole("Role.Core.Player");
        public static readonly EntityRole EntityRole = new EntityRole("Role.Core.Entity");
        public static readonly EntityRole ItemRole = new EntityRole("Role.Core.Item");

        public static readonly EntityRole ContainedItemRole = new EntityRole("Role.Core.ContainedItem");

        public CoreModule()
        {
            Id = CoreModule.ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Metaverse";
            Description = "Provides common item traits and system behaviours";
            IsFrameworkModule = true;

            RequireRole(CoreModule.EntityRole);
            RequireRole(CoreModule.PlayerRole).WithImpliedRole(CoreModule.EntityRole);
            RequireRole(CoreModule.ItemRole).WithImpliedRole(CoreModule.EntityRole);
        }

        [EntityRoleInitializer("Role.Core.Item")]
        protected void InitializeItemRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                 IModuleInitializer<TGameContext> initializer,
                                                                 EntityRole r)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(CoreModule.WorldItemComponentsId, -19_999, RegisterSharedItemComponents<TGameContext, TItemId>);
        }

        [EntityRoleInitializer("Role.Core.Entity")]
        protected void InitializeEntityRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                   IModuleInitializer<TGameContext> initializer,
                                                                   EntityRole r)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(CoreModule.CommonComponentsId, -20_000, RegisterCoreComponents);
            entityContext.Register(CoreModule.DestroyCascadingItemsSystemId, 0, RegisterCascadingDestructionSystems);
            entityContext.Register(CoreModule.DestroyItemsSystemId, int.MaxValue, RegisterEntityCleanupSystems);
        }

        [EntityRoleInitializer("Role.Core.Player")]
        protected void InitializePlayerRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                   IModuleInitializer<TGameContext> initializer,
                                                                   EntityRole r)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(CoreModule.PlayerComponentsId, -20_000, RegisterPlayerComponents);
        }

        [EntityRoleInitializer("Role.Core.ContainedItem")]
        protected void InitializeContainedItemRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                          IModuleInitializer<TGameContext> initializer,
                                                                          EntityRole r)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(CoreModule.ContainedComponentsId, -20_000, RegisterContainedItemComponents);
        }

        void RegisterCascadingDestructionSystems<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IGameLoopSystemRegistration<TGameContext> context,
                                                                        EntityRegistry<TItemId> registry,
                                                                        ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var markCascades = registry.BuildSystem()
                                       .WithContext<TGameContext>()
                                       .CreateSystem<CascadingDestroyedMarker>(DestroyedEntitiesSystem.SchedulePreviouslyMarkedItemsForDestruction);
            context.AddFixedStepHandlers(markCascades);
        }

        void RegisterEntityCleanupSystems<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                 IGameLoopSystemRegistration<TGameContext> context,
                                                                 EntityRegistry<TItemId> registry,
                                                                 ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var deleteMarkedEntitiesSystem = new DestroyedEntitiesSystem<TItemId>(registry);
            context.AddFixedStepHandlers(deleteMarkedEntitiesSystem.DeleteMarkedEntities);
        }

        void RegisterCoreComponents<TItemId>(IServiceResolver serviceResolver,
                                                           EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            // Entites are not destroyed immediately, instead we wait until the end of the turn to prune them.
            registry.RegisterFlag<DestroyedMarker>();
            registry.RegisterFlag<CascadingDestroyedMarker>();
        }

        void RegisterSharedItemComponents<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                 EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            // All entities carry a reference to their trait declaration with them. This allows
            // systems to lookup traits and to perform actions on them.
            registry.RegisterNonConstructable<ItemDeclarationHolder<TGameContext, TItemId>>();

            registry.RegisterNonConstructable<StackCount>();
            registry.RegisterNonConstructable<Weight>();
            registry.RegisterNonConstructable<Temperature>();
            registry.RegisterNonConstructable<Durability>();
            registry.RegisterNonConstructable<ItemCharge>();
        }

        void RegisterPlayerComponents<TItemId>(IServiceResolver serviceResolver,
                                               EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterFlag<PlayerTag>();
        }

        void RegisterContainedItemComponents<TItemId>(IServiceResolver serviceResolver,
                                                      EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<ContainerEntityMarker<TItemId>>();
        }
    }
}