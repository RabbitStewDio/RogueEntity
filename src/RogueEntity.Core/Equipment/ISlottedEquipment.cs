using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Equipment
{
    /// <summary>
    ///   A slotted equipment system that allows to equip items into one ore more predefined slots.
    ///   This system enables RPG style inventories. 
    /// </summary>
    /// <typeparam name="TGameContext"></typeparam>
    /// <typeparam name="TItemId"></typeparam>
    public interface ISlottedEquipment<TGameContext, TItemId> : IEnumerable<EquippedItem<TItemId>>,
                                                                IEquatable<ISlottedEquipment<TGameContext, TItemId>>
        where TItemId : IEntityKey
    {
        ReadOnlyListWrapper<EquipmentSlot> AvailableSlots { get; }
        Weight TotalWeight { get; }
        Weight MaximumCarryWeight { get; }
        
        bool TryQuery(EquipmentSlot slot, out EquippedItem<TItemId> item);
        bool TryUnequipItem(TGameContext context, TItemId item, out EquipmentSlot slot);
        bool TryUnequipItemAt(TGameContext context, TItemId item, EquipmentSlot slot);

        bool TryEquipItem(TGameContext context,
                          TItemId item,
                          out TItemId remainderItem,
                          Optional<EquipmentSlot> desiredSlot,
                          out EquipmentSlot actualSlot,
                          bool ignoreWeightLimits = false);

        bool TryReEquipItem(TGameContext context,
                            in TItemId targetItem,
                            EquipmentSlot slot);
    }
}