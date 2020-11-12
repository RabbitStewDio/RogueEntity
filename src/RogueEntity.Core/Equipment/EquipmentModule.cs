using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Equipment
{
    [Module]
    public class EquipmentModule : ModuleBase
    {
        public static readonly EntitySystemId ContainedItemsComponentId = new EntitySystemId("Entities.Core.Equipment.ContainedItem");
        public static readonly EntitySystemId ContainerComponentId = new EntitySystemId("Entities.Core.Equipment.Container");
        public static readonly EntitySystemId CascadingDestructionSystemId = new EntitySystemId("System.Core.Equipment.CascadingDestruction");

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
        protected void InitializeContainedItemRole<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                          IModuleInitializer<TGameContext> initializer,
                                                                          EntityRole r)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(ContainedItemsComponentId, -19000, RegisterContainedItemEntities);
        }

        [EntityRelationInitializer("Relation.Core.Equipment")]
        protected void InitializeContainerEntities<TGameContext, TActorId, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                    IModuleInitializer<TGameContext> initializer,
                                                                                    EntityRelation r)
            where TActorId : IEntityKey
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(ContainerComponentId, -19000, RegisterContainerEntities<TActorId, TItemId>);
            entityContext.Register(CascadingDestructionSystemId, 100_000_000, RegisterCascadingDestruction<TGameContext, TActorId, TItemId>);
        }

        void RegisterCascadingDestruction<TGameContext, TActorId, TItemId>(in ModuleInitializationParameter initParameter,
                                                                           IGameLoopSystemRegistration<TGameContext> context,
                                                                           EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            var itemResolver = initParameter.ServiceResolver.Resolve<IItemResolver<TGameContext, TItemId>>();
            var system = new DestroyContainerContentsSystem<TGameContext, TActorId, TItemId>(itemResolver);
            var action = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<DestroyedMarker, SlottedEquipmentData<TItemId>>(system.MarkDestroyedContainerEntities);
            context.AddInitializationStepHandler(action);
            context.AddFixedStepHandlers(action);
        }

        void RegisterContainedItemEntities<TItemId>(in ModuleInitializationParameter initParameter,
                                                    EntityRegistry<TItemId> registry)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            registry.RegisterNonConstructable<SlottedEquipmentData<TItemId>>();
        }

        void RegisterContainerEntities<TActorId, TItemId>(in ModuleInitializationParameter initParameter,
                                                          EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            registry.RegisterNonConstructable<SlottedEquipmentData<TItemId>>();
        }
    }
}