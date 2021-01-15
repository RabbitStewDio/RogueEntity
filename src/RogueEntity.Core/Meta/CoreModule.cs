using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
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
        public static readonly EntitySystemId WorldItemComponentsId = "Entities.Core.Item";
        public static readonly EntitySystemId ContainedComponentsId = "Entities.Core.ContainedItem";

        public static readonly EntitySystemId DestroyItemsSystemId = "System.Core.Entity.DestroyMarked";
        public static readonly EntitySystemId DestroyCascadingItemsSystemId = "System.Core.Entity.DestroyCascadingMarked";

        public static readonly EntityRole EntityRole = new EntityRole("Role.Core.Entity");
        public static readonly EntityRole ItemRole = new EntityRole("Role.Core.Item");

        public static readonly EntityRole ContainedItemRole = new EntityRole("Role.Core.ContainedItem");

        public CoreModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Metaverse";
            Description = "Provides common item traits and system behaviours";
            IsFrameworkModule = true;

            RequireRole(EntityRole);
            RequireRole(ItemRole).WithImpliedRole(EntityRole);
            RequireRole(ContainedItemRole).WithImpliedRole(EntityRole);
        }

        [EntityRoleInitializer("Role.Core.Item")]
        protected void InitializeItemRole< TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                 IModuleInitializer initializer,
                                                                 EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(WorldItemComponentsId, -19_999, RegisterSharedItemComponents);
        }

        [EntityRoleInitializer("Role.Core.Entity")]
        protected void InitializeEntityRole< TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                   IModuleInitializer initializer,
                                                                   EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(CommonComponentsId, -20_000, RegisterCoreComponents);
            entityContext.Register(DestroyCascadingItemsSystemId, 0, RegisterCascadingDestructionSystems);
            entityContext.Register(DestroyItemsSystemId, int.MaxValue, RegisterEntityCleanupSystems);
        }

        [EntityRoleInitializer("Role.Core.ContainedItem")]
        protected void InitializeContainedItemRole< TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                          IModuleInitializer initializer,
                                                                          EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(ContainedComponentsId, -20_000, RegisterContainedItemComponents);
        }

        void RegisterCascadingDestructionSystems< TItemId>(in ModuleInitializationParameter initParameter,
                                                                        IGameLoopSystemRegistration context,
                                                                        EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var markCascades = registry.BuildSystem()
                                       .WithoutContext()
                                       .WithInputParameter<CascadingDestroyedMarker>()
                                       .CreateSystem(DestroyedEntitiesSystem<TItemId>.SchedulePreviouslyMarkedItemsForDestruction);
            context.AddFixedStepHandlers(markCascades);
        }

        void RegisterEntityCleanupSystems< TItemId>(in ModuleInitializationParameter initParameter,
                                                                 IGameLoopSystemRegistration context,
                                                                 EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var deleteMarkedEntitiesSystem = new DestroyedEntitiesSystem<TItemId>(registry);
            context.AddInitializationStepHandler(deleteMarkedEntitiesSystem.DeleteMarkedEntities);
            context.AddFixedStepHandlers(deleteMarkedEntitiesSystem.DeleteMarkedEntities);
        }

        void RegisterCoreComponents<TItemId>(in ModuleInitializationParameter initParameter,
                                             EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            // Entites are not destroyed immediately, instead we wait until the end of the turn to prune them.
            registry.RegisterFlag<DestroyedMarker>();
            registry.RegisterFlag<CascadingDestroyedMarker>();
        }

        void RegisterSharedItemComponents< TItemId>(in ModuleInitializationParameter initParameter,
                                                                 EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            // All entities carry a reference to their trait declaration with them. This allows
            // systems to lookup traits and to perform actions on them.
            registry.RegisterNonConstructable<ItemDeclarationHolder< TItemId>>();

            registry.RegisterNonConstructable<StackCount>();
            registry.RegisterNonConstructable<Weight>();
            registry.RegisterNonConstructable<Temperature>();
            registry.RegisterNonConstructable<Durability>();
            registry.RegisterNonConstructable<ItemCharge>();
        }

        void RegisterContainedItemComponents<TItemId>(in ModuleInitializationParameter initParameter,
                                                      EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<ContainerEntityMarker<TItemId>>();
        }
    }
}