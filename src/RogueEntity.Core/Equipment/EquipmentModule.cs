using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Equipment
{
    [Module]
    public class EquipmentModule : ModuleBase
    {
        public static readonly EntitySystemId ContainedItemsComponentId = new EntitySystemId("Entities.Core.Equipment.ContainedItem");
        public static readonly EntitySystemId ContainerComponentId = new EntitySystemId("Entities.Core.Equipment.Container");

        public static readonly EntityRole EquipmentContainerRole = new EntityRole("Role.Core.Equipment.Container");
        public static readonly EntityRole EquipmentContainedItemRole = new EntityRole("Role.Core.Equipment.ContainedItem");
        public static readonly EntityRelation CanEquipRelation = new EntityRelation("Relation.Core.Equipment", EquipmentContainerRole, EquipmentContainedItemRole);
        
        public EquipmentModule()
        {
            Id = "Core.Equipment";
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Equipment";
            Description = "Provides base classes and behaviours for equipping items";
            IsFrameworkModule = true;

            RequireRole(EquipmentContainerRole).WithDependencyOn(CoreModule.ModuleId);
            RequireRole(EquipmentContainedItemRole).WithImpliedRole(CoreModule.ContainedItemRole).WithDependencyOn(CoreModule.ModuleId);
            RequireRelation(CanEquipRelation);
        }

        [EntityRoleInitializer("Role.Core.Equipment.ContainedItem")]
        protected void InitializeContainedItemRole<TGameContext, TItemId>(IServiceResolver serviceResolver, 
                                                                          IModuleInitializer<TGameContext> initializer,
                                                                          EntityRole r)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(EquipmentModule.ContainedItemsComponentId, -19000, RegisterContainedItemEntities<TItemId>);
        }

        [EntityRelationInitializer("Relation.Core.Equipment")]
        protected void InitializeContainerEntities<TGameContext, TActorId, TItemId>(IServiceResolver serviceResolver, 
                                                                                    IModuleInitializer<TGameContext> initializer,
                                                                                    EntityRelation r)
            where TActorId : IEntityKey
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(EquipmentModule.ContainerComponentId, -19000, RegisterContainerEntities<TActorId, TItemId>);
        }

        void RegisterContainedItemEntities<TItemId>(IServiceResolver serviceResolver,
                                                    EntityRegistry<TItemId> registry)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            registry.RegisterNonConstructable<SlottedEquipmentData<TItemId>>();
        }

        void RegisterContainerEntities<TActorId, TItemId>(IServiceResolver serviceResolver,
                                                          EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            registry.RegisterNonConstructable<SlottedEquipmentData<TItemId>>();
        }
    }
}