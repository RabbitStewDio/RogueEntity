﻿using EnTTSharp.Entities;
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
        protected void InitializeContainerEntities<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                      IModuleInitializer initializer,
                                                                      EntityRelation r)
            where TActorId : struct, IEntityKey
            where TItemId : struct, IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(ContainerComponentId, -19000, RegisterEntities<TActorId, TItemId>);
            entityContext.Register(CascadingDestructionSystemId, 100_000_000, RegisterCascadingDestruction<TActorId, TItemId>);
        }

        void RegisterEntities<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                 EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
            where TItemId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<ListInventoryData<TActorId, TItemId>>();
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
                                 .WithInputParameter<DestroyedMarker, ListInventoryData<TActorId, TItemId>>()
                                 .CreateSystem(system.MarkDestroyedContainerEntities);
            context.AddInitializationStepHandlerSystem(action);
            context.AddFixedStepHandlerSystem(action);
        }
    }
}
