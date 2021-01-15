using EnTTSharp.Entities;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Equipment
{
    public static class SlottedEquipmentExtensions
    {
        public static bool TryEquipItem< TItemId>(this ISlottedEquipment< TItemId> equipment,
                                                               TItemId item,
                                                               out TItemId modifiedItem,
                                                               out EquipmentSlot actualSlot,
                                                               bool ignoreWeight = false)
            where TItemId : IEntityKey
        {
            return equipment.TryEquipItem(item, out modifiedItem, Optional.Empty(), out actualSlot, ignoreWeight);
        }

        public static bool TryUnequipItem< TItemId>(this ISlottedEquipment< TItemId> equipment,
                                                                 TItemId item)
            where TItemId : IEntityKey
        {
            return equipment.TryUnequipItem(item, out _);
        }

        public static BufferList<EquippedItem<TItemId>> QueryItems< TItemId>(this ISlottedEquipment< TItemId> equipment,
                                                                                          BufferList<EquippedItem<TItemId>> data = null)
            where TItemId : IEntityKey
        {
            data = BufferList.PrepareBuffer(data);

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

        public static BufferList<EquippedItem<TItemId>> QueryEquipmentSlots< TItemId>(this ISlottedEquipment< TItemId> equipment,
                                                                                                   BufferList<EquippedItem<TItemId>> data = null)
            where TItemId : IEntityKey
        {
            data = BufferList.PrepareBuffer(data);

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
