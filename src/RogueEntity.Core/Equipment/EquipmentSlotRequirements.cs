using System.Collections.Generic;
using System.Runtime.Serialization;
using MessagePack;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Equipment
{
    /// <summary>
    ///   Declares how an item can be equipped. All required slots must be
    ///   available for the item to be used. This collection can be empty
    ///   if the item only relies on acceptable slots.
    ///
    ///   Acceptable slots define a single acceptable slot the item requires
    ///   in addition to any required slots.
    ///
    ///   For an two handed weapon, the required-slots should be set to the
    ///   main and off-hand weapon slot.
    ///
    ///   For rings that can be worn on either hand, leave the required slot
    ///   empty and fill the acceptable slots collection instead.
    /// </summary>
    [DataContract]
    [MessagePackObject]
    public class EquipmentSlotRequirements
    {
        [DataMember(Name = nameof(RequiredSlots), Order = 0)]
        [Key(0)]
        readonly List<EquipmentSlot> requiredSlots;

        [DataMember(Name = nameof(AcceptableSlots), Order = 1)]
        [Key(1)]
        readonly List<EquipmentSlot> acceptableSlots;

        public ReadOnlyListWrapper<EquipmentSlot> RequiredSlots => requiredSlots;
        public ReadOnlyListWrapper<EquipmentSlot> AcceptableSlots => acceptableSlots;

        public EquipmentSlotRequirements()
        {
            this.requiredSlots = new List<EquipmentSlot>();
            this.acceptableSlots = new List<EquipmentSlot>();
        }

        public EquipmentSlotRequirements WithRequiredSlots(params EquipmentSlot[] slots)
        {
            foreach (var s in slots)
            {
                acceptableSlots.Remove(s);

                if (!requiredSlots.Contains(s))
                {
                    requiredSlots.Add(s);
                }
            }

            return this;
        }

        public EquipmentSlotRequirements WithAcceptableSlots(params EquipmentSlot[] slots)
        {
            foreach (var s in slots)
            {
                requiredSlots.Remove(s);
                if (!acceptableSlots.Contains(s))
                {
                    acceptableSlots.Add(s);
                }
            }

            return this;
        }

        public static EquipmentSlotRequirements ForAcceptableSlots(params EquipmentSlot[] slots)
        {
            return new EquipmentSlotRequirements().WithAcceptableSlots(slots);
        }

        public static EquipmentSlotRequirements ForRequiredSlots(params EquipmentSlot[] slots)
        {
            return new EquipmentSlotRequirements().WithRequiredSlots(slots);
        }
    }
}