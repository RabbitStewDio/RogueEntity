using System.Collections.Generic;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Equipment
{
    public static class SlottedEquipmentExtensions
    {
        public static bool TryEquipItem<TGameContext, TItemId>(this ISlottedEquipment<TGameContext, TItemId> equipment,
                                                               TGameContext context,
                                                               TItemId item,
                                                               out TItemId modifiedItem,
                                                               out EquipmentSlot actualSlot,
                                                               bool ignoreWeight = false)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return equipment.TryEquipItem(context, item, out modifiedItem, Optional.Empty<EquipmentSlot>(), out actualSlot, ignoreWeight);
        }

        public static bool TryUnequipItem<TGameContext, TItemId>(this ISlottedEquipment<TGameContext, TItemId> equipment,
                                                                 TGameContext context,
                                                                 TItemId item)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            return equipment.TryUnequipItem(context, item, out _);
        }

        public static List<EquippedItem<TItemId>> QueryItems<TGameContext, TItemId>(this ISlottedEquipment<TGameContext, TItemId> equipment,
                                                                                    List<EquippedItem<TItemId>> data = null)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            if (data == null)
            {
                data = new List<EquippedItem<TItemId>>();
            }
            else
            {
                data.Clear();
            }

            foreach (var slot in equipment.AvailableSlots)
            {
                if (equipment.TryQuery(slot, out var item) &&
                    item.PrimarySlot == slot)
                {
                    data.Add(item);
                }
            }

            return data;
        }

        public static List<EquippedItem<TItemId>> QueryEquipmentSlots<TGameContext, TItemId>(this ISlottedEquipment<TGameContext, TItemId> equipment,
                                                                                             List<EquippedItem<TItemId>> data = null)
            where TItemId : IBulkDataStorageKey<TItemId>
        {
            if (data == null)
            {
                data = new List<EquippedItem<TItemId>>();
            }
            else
            {
                data.Clear();
            }

            foreach (var slot in equipment.AvailableSlots)
            {
                if (equipment.TryQuery(slot, out var item))
                {
                    data.Add(item);
                }
            }

            return data;
        }
    }
}