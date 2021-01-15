using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Equipment
{
    /// <summary>
    ///   A slotted equipment system that allows to equip items into one ore more predefined slots.
    ///   This system enables RPG style inventories. 
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public interface ISlottedEquipment<TItemId> : IEnumerable<EquippedItem<TItemId>>,
                                                  IEquatable<ISlottedEquipment<TItemId>>
        where TItemId : IEntityKey
    {
        ReadOnlyListWrapper<EquipmentSlot> AvailableSlots { get; }
        Weight TotalWeight { get; }
        Weight MaximumCarryWeight { get; }

        bool TryQuery(EquipmentSlot slot, out EquippedItem<TItemId> item);
        bool TryUnequipItem(TItemId item, out EquipmentSlot slot);
        bool TryUnequipItemAt(TItemId item, EquipmentSlot slot);

        bool TryEquipItem(TItemId item,
                          out TItemId remainderItem,
                          Optional<EquipmentSlot> desiredSlot,
                          out EquipmentSlot actualSlot,
                          bool ignoreWeightLimits = false);

        bool TryReEquipItem(in TItemId targetItem,
                            EquipmentSlot slot);
    }
}
