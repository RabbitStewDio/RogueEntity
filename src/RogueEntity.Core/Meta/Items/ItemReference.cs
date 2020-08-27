using System;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    /// <summary>
    /// Describes an item at a given map or inventory position.
    ///
    /// Type is stored at the highest 1 bit of the 32-bit vector.
    /// If type indicates a reference type, the remaining 31 bits
    /// are used as item-id. 
    ///
    /// If type indicates either a stacked or simple stateful item
    /// the following layout is used:
    /// <pre>
    /// - State    16
    ///   - type Simple:  item dependent: Durability, flags etc.
    ///   - type Stacked: stack count
    /// 
    /// - Item ID        14
    ///   - type  
    /// - Type            2 
    ///   1     referenced, not stacking        - a chest, or anything with complex state, stored in a EntityRegistry.
    ///   00     Simple Stateful, not stacking   - a sword
    ///   01     stateless, stacking             - Gold coins
    ///   
    /// allows 8k different item types 
    /// 
    /// </pre>
    /// </summary>
    public readonly struct ItemReference : IEquatable<ItemReference>, IBulkDataStorageKey<ItemReference>
    {
        public static int MaxAge => 7;
        readonly uint data;

        ItemReference(uint data)
        {
            this.data = data;
        }

        public bool IsEmpty => data == 0;

        public bool IsReference
        {
            get
            {
                var raw = data & 0x8000_0000;
                return raw == 0;
            }
        }

        public int Data
        {
            get
            {
                if (IsReference)
                {
                    return 0;
                }

                return (ushort) (data & 0xFFFF);
            }
        }

        public byte Age
        {
            get { return (byte) ((data & 0x7000_0000) >> 28); }
        }

        public int Key
        {
            get { return (int) (data & 0xFFF_FFFF); }
        }

        public uint ItemId
        {
            get
            {
                if (IsReference)
                {
                    return data;
                }

                var stack = (data >> 16) & 0x7FFF;
                return stack;
            }
        }

        public int BulkItemId
        {
            get
            {
                if (IsReference)
                {
                    return 0;
                }

                return (int) ((data & 0xFFFF_0000) >> 16);
            }
        }

        public ItemReference WithData(int newData)
        {
            if (IsReference)
            {
                throw new InvalidOperationException();
            }

            if (newData < 0 || newData > ushort.MaxValue)
            {
                throw new InvalidOperationException();
            }
            return FromBulkItem((ushort) ItemId, (ushort) newData);
        }

        public static ItemReference FromReferencedItem(int age, int key)
        {
            if (age < 0 || age > MaxAge) throw new ArgumentException();
            if (key < 0 || key > 0x0FFF_FFFF) throw new ArgumentException();

            var rawData = 0x8000_0000;
            rawData |= (uint)(age << 28);
            rawData |= (uint)(key);
            return new ItemReference(rawData);
        }

        public static ItemReference FromBulkItem(ushort itemId, ushort data)
        {
            if (itemId > short.MaxValue)
                throw new ArgumentException();

            var rawData = 0u;
            rawData |= (uint) (itemId << 16);
            rawData |= data;
            return new ItemReference(rawData);
        }

        public bool Equals(ItemReference other)
        {
            return data == other.data;
        }

        public bool Equals(IEntityKey obj)
        {
            return obj is ItemReference other && Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is ItemReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) data;
        }

        public static bool operator ==(ItemReference left, ItemReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemReference left, ItemReference right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            if (IsReference)
            {
                return $"Item[Ref]{ItemId & 0x7FFF:X8}";
            }
            return $"Item[Bulk]{ItemId:X4}[{Data:X4}]";
        }
    }
}