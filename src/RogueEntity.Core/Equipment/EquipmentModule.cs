﻿using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Meta.Base;

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
        protected void InitializeContainedItemRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                            IModuleInitializer initializer,
                                                            EntityRole r)
            where TItemId : struct, IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(ContainedItemsComponentId, -19000, RegisterContainedItemEntities);
        }

        [EntityRelationInitializer("Relation.Core.Equipment")]
        protected void InitializeContainerEntities<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                      IModuleInitializer initializer,
                                                                      EntityRelation r)
            where TActorId : struct, IEntityKey
            where TItemId : struct, IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(ContainerComponentId, -19000, RegisterContainerEntities<TActorId, TItemId>);
            entityContext.Register(CascadingDestructionSystemId, 100_000_000, RegisterCascadingDestruction<TActorId, TItemId>);
        }

        void RegisterCascadingDestruction<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                             IGameLoopSystemRegistration context,
                                                             EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
            where TItemId : struct, IEntityKey
        {
            var itemResolver = initParameter.ServiceResolver.Resolve<IItemResolver<TItemId>>();
            var system = new DestroyContainerContentsSystem<TActorId, TItemId>(itemResolver);
            var action = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<DestroyedMarker, SlottedEquipmentData<TItemId>>()
                                 .CreateSystem(system.MarkDestroyedContainerEntities);

            context.AddInitializationStepHandlerSystem(action);
            context.AddFixedStepHandlerSystem(action);
        }

        void RegisterContainedItemEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                    EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<SlottedEquipmentData<TItemId>>();
        }

        void RegisterContainerEntities<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                          EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
            where TItemId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<SlottedEquipmentData<TItemId>>();
        }
    }
}
