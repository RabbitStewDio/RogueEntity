﻿using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Meta
{
    public static class CoreModule
    {
        public const string ModuleId = "Core.MetaModule";

        public static readonly EntitySystemId CoreTraitsId = "Core.CoreEntities";
        public static readonly EntitySystemId ItemTraitsId = "Core.ItemEntities";
        public static readonly EntitySystemId DestroyItemsId = "Core.Entity.DestroyMarked";
        public static readonly EntitySystemId DestroyCascadingItemsId = "Core.Entity.DestroyCascadingMarked";
    }

    public class CoreModule<TGameContext> : ModuleBase<TGameContext>
    {
        public CoreModule()
        {
            Id = CoreModule.ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Metaverse";
            Description = "Provides common item traits and system behaviours";
        }

        [ModuleEntityInitializer]
        protected void RegisterAll<TItemId>(TGameContext context, IModuleInitializer<TGameContext> initializer) where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(CoreModule.CoreTraitsId, -20_000, RegisterCoreTraits);
            entityContext.Register(CoreModule.ItemTraitsId, -19_999, RegisterSharedItemTraits);

            entityContext.Register(CoreModule.DestroyCascadingItemsId, 0, RegisterEntityCleanupSystems);
            entityContext.Register(CoreModule.DestroyItemsId, int.MaxValue, RegisterEntityCleanupSystems);
        }


        protected void RegisterCascadingDestructionSystems<TItemId>(IGameLoopSystemRegistration<TGameContext> context,
                                                                    EntityRegistry<TItemId> registry,
                                                                    ICommandHandlerRegistration<TGameContext, TItemId> handler) where TItemId : IEntityKey
        {
            var markCascades = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<CascadingDestroyedMarker>(DestroyedEntitiesSystem.SchedulePreviouslyMarkedItemsForDestruction);
            context.AddFixedStepHandlers(markCascades);
        }

        protected void RegisterEntityCleanupSystems<TItemId>(IGameLoopSystemRegistration<TGameContext> context,
                                                             EntityRegistry<TItemId> registry,
                                                             ICommandHandlerRegistration<TGameContext, TItemId> handler) where TItemId : IEntityKey
        {
            var deleteMarkedEntitiesSystem = new DestroyedEntitiesSystem<TItemId>(registry);
            context.AddFixedStepHandlers(deleteMarkedEntitiesSystem.DeleteMarkedEntities);
        }

        protected void RegisterCoreTraits<TItemId>(EntityRegistry<TItemId> registry) where TItemId : IEntityKey
        {
            // Entites are not destroyed immediately, instead we wait until the end of the turn to prune them.
            registry.RegisterFlag<DestroyedMarker>();
            registry.RegisterFlag<CascadingDestroyedMarker>();

            // All entities carry a reference to their trait declaration with them. This allows
            // systems to lookup traits and to perform actions on them.
            registry.RegisterNonConstructable<ItemDeclarationHolder<TGameContext, TItemId>>();
        }

        protected void RegisterSharedItemTraits<TItemId>(EntityRegistry<TItemId> registry) where TItemId : IEntityKey
        {
            registry.RegisterFlag<PlayerTag>();
            registry.RegisterNonConstructable<StackCount>();
            registry.RegisterNonConstructable<Weight>();
            registry.RegisterNonConstructable<Temperature>();
            registry.RegisterNonConstructable<Durability>();
            registry.RegisterNonConstructable<ItemCharge>();
            registry.RegisterNonConstructable<SensoryResistance>();
        }
    }
}