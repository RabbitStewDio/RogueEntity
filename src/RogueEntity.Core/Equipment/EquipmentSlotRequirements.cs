using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Api.Utils;
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
    [EntityComponent(EntityConstructor.NonConstructable)]
    public readonly struct EquipmentSlotRequirements : IEquatable<EquipmentSlotRequirements>
    {
        [DataMember(Name = nameof(RequiredSlots), Order = 0)]
        [Key(0)]
        readonly List<EquipmentSlot> requiredSlots;

        [DataMember(Name = nameof(AcceptableSlots), Order = 1)]
        [Key(1)]
        readonly List<EquipmentSlot> acceptableSlots;

        [DataMember(Name = nameof(AllowStacks), Order = 2)]
        [Key(2)]
        public bool AllowStacks { get; }
        
        [IgnoreMember]
        [IgnoreDataMember]
        public ReadOnlyListWrapper<EquipmentSlot> RequiredSlots => requiredSlots;
        
        [IgnoreMember]
        [IgnoreDataMember]
        public ReadOnlyListWrapper<EquipmentSlot> AcceptableSlots => acceptableSlots;

        [SerializationConstructor]
        internal EquipmentSlotRequirements(List<EquipmentSlot> requiredSlots, List<EquipmentSlot> acceptableSlots, bool allowStacks)
        {
            this.requiredSlots = requiredSlots;
            this.acceptableSlots = acceptableSlots;
            AllowStacks = allowStacks;
        }

        public EquipmentSlotRequirements WithRequiredSlots(params EquipmentSlot[] slots)
        {
            var acceptable = new List<EquipmentSlot>(acceptableSlots);
            var required = new List<EquipmentSlot>(requiredSlots);
            foreach (var s in slots)
            {
                acceptable.Remove(s);

                if (!required.Contains(s))
                {
                    required.Add(s);
                }
            }

            return new EquipmentSlotRequirements(required, acceptable, AllowStacks);
        }

        public EquipmentSlotRequirements WithAcceptableSlots(params EquipmentSlot[] slots)
        {
            var acceptable = new List<EquipmentSlot>(acceptableSlots);
            var required = new List<EquipmentSlot>(requiredSlots);
            foreach (var s in slots)
            {
                required.Remove(s);
                if (!acceptable.Contains(s))
                {
                    acceptable.Add(s);
                }
            }

            return new EquipmentSlotRequirements(required, acceptable, AllowStacks);
        }
        
        public EquipmentSlotRequirements WithStacksAllowed()
        {
            return new EquipmentSlotRequirements(requiredSlots, acceptableSlots, true);
        }
        
        public EquipmentSlotRequirements WithStacksProhibited()
        {
            return new EquipmentSlotRequirements(requiredSlots, acceptableSlots, false);
        }
        
        public static EquipmentSlotRequirements Create()
        {
            return new EquipmentSlotRequirements(new List<EquipmentSlot>(), new List<EquipmentSlot>(), false);
        }

        public static EquipmentSlotRequirements ForAcceptableSlots(params EquipmentSlot[] slots)
        {
            return Create().WithAcceptableSlots(slots);
        }

        public static EquipmentSlotRequirements ForRequiredSlots(params EquipmentSlot[] slots)
        {
            return Create().WithRequiredSlots(slots);
        }

        public bool Equals(EquipmentSlotRequirements other)
        {
            return CoreExtensions.EqualsList(requiredSlots, other.requiredSlots) && CoreExtensions.EqualsList(acceptableSlots, other.acceptableSlots) && AllowStacks == other.AllowStacks;
        }

        public override bool Equals(object obj)
        {
            return obj is EquipmentSlotRequirements other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (requiredSlots != null ? requiredSlots.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (acceptableSlots != null ? acceptableSlots.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ AllowStacks.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(EquipmentSlotRequirements left, EquipmentSlotRequirements right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EquipmentSlotRequirements left, EquipmentSlotRequirements right)
        {
            return !left.Equals(right);
        }
    }
}