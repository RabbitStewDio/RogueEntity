using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;

namespace RogueEntity.Core.Inventory
{
    /// <summary>
    ///   This module does not have enough information to directly register entities as the inventory
    ///   may reach across different entity systems to work. (NPC entity with its own entity system
    ///   might hold references to item-entities from a different system.)
    /// 
    ///   The actual game content modules needs to specify both the owner of the inventory and the
    ///   items contained in the inventory. 
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    public class InventoryModule<TGameContext> : ModuleBase<TGameContext>
    {
        public InventoryModule()
        {
            Id = "Core.Inventory";
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Inventory";
            Description = "Provides base classes and behaviours for carrying items in a list-based inventory";
        }

        public static void RegisterEntities<TActorId, TItemId>(EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<ListInventoryData<TActorId, TItemId>>();
        }
    }
}