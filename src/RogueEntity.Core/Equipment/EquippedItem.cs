﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Equipment
{
    [Serializable]
    [DataContract]
    [MessagePackObject]
    public readonly struct EquippedItem<TItemId> : IEquatable<EquippedItem<TItemId>>
    {
        static readonly EqualityComparer<TItemId> ItemEquality = EqualityComparer<TItemId>.Default;
        [DataMember]
        [Key(0)]
        public readonly TItemId Reference;

        [DataMember]
        [Key(1)]
        public readonly EquipmentSlot PrimarySlot;

        public EquippedItem(TItemId reference, EquipmentSlot primarySlot)
        {
            this.Reference = reference;
            this.PrimarySlot = primarySlot ?? throw new ArgumentNullException();
        }

        public override string ToString()
        {
            return $"{nameof(Reference)}: {Reference}, {nameof(PrimarySlot)}: {PrimarySlot}";
        }

        public bool Equals(EquippedItem<TItemId> other)
        {
            return ItemEquality.Equals(Reference, other.Reference) && 
                   Equals(PrimarySlot, other.PrimarySlot);
        }

        public override bool Equals(object obj)
        {
            return obj is EquippedItem<TItemId> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Reference.GetHashCode() * 397) ^ PrimarySlot.GetHashCode();
            }
        }

        public static bool operator ==(EquippedItem<TItemId> left, EquippedItem<TItemId> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EquippedItem<TItemId> left, EquippedItem<TItemId> right)
        {
            return !left.Equals(right);
        }

        sealed class OrderRelationalComparer : IComparer<EquippedItem<TItemId>>
        {
            public int Compare(EquippedItem<TItemId> x, EquippedItem<TItemId> y)
            {
                return x.PrimarySlot.Order.CompareTo(y.PrimarySlot.Order);
            }
        }

        public static IComparer<EquippedItem<TItemId>> OrderComparer { get; } = new OrderRelationalComparer();


    }
}