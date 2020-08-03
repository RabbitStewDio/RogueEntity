using System.Collections.Generic;

namespace RogueEntity.Core.Equipment
{
    public delegate bool EquipmentSlotRegistryLookup(string id, out EquipmentSlot slot);

    public class EquipmentSlotRegistry
    {
        readonly Dictionary<string, EquipmentSlot> equipmentSlots;

        public EquipmentSlotRegistry()
        {
            equipmentSlots = new Dictionary<string, EquipmentSlot>();
        }

        public bool TryGet(string id, out EquipmentSlot slot)
        {
            return equipmentSlots.TryGetValue(id, out slot);
        }

        public void Register(EquipmentSlot slot)
        {
            equipmentSlots[slot.Id] = slot;
        }
    }
}