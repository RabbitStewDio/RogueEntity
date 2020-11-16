using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Base;

namespace RogueEntity.Core.Inventory
{
    [Module]
    public class InventoryModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Inventory";

        public static readonly EntitySystemId ContainedItemsComponentId = new EntitySystemId("Entities.Core.Inventory.ContainedItem");
        public static readonly EntitySystemId ContainerComponentId = new EntitySystemId("Entities.Core.Inventory.Container");
        public static readonly EntitySystemId CascadingDestructionSystemId = new EntitySystemId("System.Core.Inventory.CascadingDestruction");

        public static readonly EntityRole ContainerRole = new EntityRole("Role.Core.Inventory.Container");
        public static readonly EntityRole ContainedItemRole = new EntityRole("Role.Core.Inventory.ContainedItem");
        public static readonly EntityRelation ContainsRelation = new EntityRelation("Relation.Core.Inventory", ContainerRole, ContainedItemRole);

        public InventoryModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Inventory";
            Description = "Provides base classes and behaviours for carrying items in a list-based inventory";
            IsFrameworkModule = true;

            RequireRole(ContainerRole).WithDependencyOn(CoreModule.ModuleId);
            RequireRole(ContainedItemRole).WithImpliedRole(CoreModule.ContainedItemRole).WithDependencyOn(CoreModule.ModuleId);

            RequireRelation(ContainsRelation);
        }

        [EntityRelationInitializer("Relation.Core.Inventory")]
        protected void InitializeContainerEntities<TGameContext, TActorId, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TActorId> initParameter,
                                                                                    IModuleInitializer<TGameContext> initializer,
                                                                                    EntityRelation r)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(ContainerComponentId, -19000, RegisterEntities<TActorId, TItemId>);
            entityContext.Register(CascadingDestructionSystemId, 100_000_000, RegisterCascadingDestruction<TGameContext, TActorId, TItemId>);
        }

        void RegisterEntities<TActorId, TItemId>(in ModuleInitializationParameter initParameter,
                                                 EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<ListInventoryData<TActorId, TItemId>>();
        }

        void RegisterCascadingDestruction<TGameContext, TActorId, TItemId>(in ModuleInitializationParameter initParameter,
                                                                           IGameLoopSystemRegistration<TGameContext> context,
                                                                           EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var itemResolver = initParameter.ServiceResolver.Resolve<IItemResolver<TGameContext, TItemId>>();
            var system = new DestroyContainerContentsSystem<TGameContext, TActorId, TItemId>(itemResolver);
            var action = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<DestroyedMarker, ListInventoryData<TActorId, TItemId>>(system.MarkDestroyedContainerEntities);
            context.AddInitializationStepHandler(action);
            context.AddFixedStepHandlers(action);
        }
    }
}