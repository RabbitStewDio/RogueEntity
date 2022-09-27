using System;
using System.Collections.Generic;
using MessagePack;

namespace RogueEntity.Core.Equipment
{
    [MessagePackFormatter(typeof(EquipmentSlotMessagePackFormatter))]
    public class EquipmentSlot : IEquatable<EquipmentSlot>
    {
        public string Id { get; }
        public string Name { get; }
        public string ShortCode { get; }
        public int Order { get; }

        public EquipmentSlot(string id, int order, string name, string shortCode)
        {
            this.Order = order;
            Id = id;
            Name = name;
            ShortCode = shortCode;
        }

        sealed class OrderRelationalComparer : IComparer<EquipmentSlot>
        {
            public int Compare(EquipmentSlot x, EquipmentSlot y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (ReferenceEquals(null, y))
                {
                    return 1;
                }

                if (ReferenceEquals(null, x))
                {
                    return -1;
                }

                return x.Order.CompareTo(y.Order);
            }
        }

        public static IComparer<EquipmentSlot> OrderComparer { get; } = new OrderRelationalComparer();

        public bool Equals(EquipmentSlot other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((EquipmentSlot) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(EquipmentSlot? left, EquipmentSlot? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EquipmentSlot? left, EquipmentSlot? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"EquipmentSlot({nameof(Name)}: {Name})";
        }
    }
}