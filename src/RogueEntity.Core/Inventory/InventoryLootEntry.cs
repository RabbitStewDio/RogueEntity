using System;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Inventory
{
    public readonly struct InventoryLootEntry : IEquatable<InventoryLootEntry>
    {
        public readonly ItemDeclarationId Item;
        public readonly float Probability;

        public InventoryLootEntry(ItemDeclarationId item, float probability = 0.1f)
        {
            this.Item = item;
            this.Probability = probability;
        }

        public bool Equals(InventoryLootEntry other)
        {
            return Item.Equals(other.Item) && Probability.Equals(other.Probability);
        }

        public override bool Equals(object obj)
        {
            return obj is InventoryLootEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Item.GetHashCode() * 397) ^ Probability.GetHashCode();
            }
        }

        public static bool operator ==(InventoryLootEntry left, InventoryLootEntry right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InventoryLootEntry left, InventoryLootEntry right)
        {
            return !left.Equals(right);
        }
    }
}