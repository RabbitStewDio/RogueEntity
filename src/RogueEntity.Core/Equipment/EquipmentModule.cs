using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;

namespace RogueEntity.Core.Equipment
{
    public class EquipmentModule<TGameContext> : ModuleBase<TGameContext>
    {
        public EquipmentModule()
        {
            Id = "Core.Equipment";
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Equipment";
            Description = "Provides base classes and behaviours for equipping items";
        }

        protected void RegisterAll<TActorId, TItemId>(TGameContext context, IModuleInitializer<TGameContext> initializer)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register("Core.Entities.Equipment", -19000, RegisterEntities<TActorId, TItemId>);
        }

        void RegisterEntities<TActorId, TItemId>(EntityRegistry<TActorId> registry) 
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<EquippedItem<TItemId>>();
            registry.RegisterNonConstructable<SlottedEquipmentData<TItemId>>();
        }
    }

}