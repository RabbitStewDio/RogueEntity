using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;

namespace RogueEntity.Core.Inventory
{
    public class InventoryModule<TGameContext> : ModuleBase<TGameContext>
    {
        public InventoryModule()
        {
            Id = "Core.Inventory";
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Inventory";
            Description = "Provides base classes and behaviours for carrying items in a list-based inventory";
        }

        protected void RegisterAll<TActorId, TItemId>(TGameContext context, IModuleInitializer<TGameContext> initializer)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register("Core.Entities.Inventory", -19_000, RegisterEntities<TActorId, TItemId>);
        }

        void RegisterEntities<TActorId, TItemId>(EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<ListInventoryData<TActorId, TItemId>>();
            registry.RegisterFlag<ContainedInInventoryMarker<TActorId, TItemId>>();
        }
    }

}