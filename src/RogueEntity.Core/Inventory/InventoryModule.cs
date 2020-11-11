using EnTTSharp.Entities;
using RogueEntity.Core.Equipment;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Inventory
{
    [Module]
    public class InventoryModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Inventory";

        public static readonly EntitySystemId ContainedItemsComponentId = new EntitySystemId("Entities.Core.Inventory.ContainedItem");
        public static readonly EntitySystemId ContainerComponentId = new EntitySystemId("Entities.Core.Inventory.Container");

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
        protected void InitializeContainerEntities<TGameContext, TActorId, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                    IModuleInitializer<TGameContext> initializer,
                                                                                    EntityRelation r)
            where TActorId : IEntityKey
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(EquipmentModule.ContainerComponentId, -19000, RegisterEntities<TActorId, TItemId>);
        }

        void RegisterEntities<TActorId, TItemId>(in ModuleInitializationParameter initParameter,
                                                 EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<ListInventoryData<TActorId, TItemId>>();
        }
    }
}