using EnTTSharp.Entities;
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

            entityContext.Register(CoreModule.DestroyItemsId, int.MaxValue, RegisterCoreSystems);
        }


        protected void RegisterCoreSystems<TItemId>(IGameLoopSystemRegistration<TGameContext> context, 
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