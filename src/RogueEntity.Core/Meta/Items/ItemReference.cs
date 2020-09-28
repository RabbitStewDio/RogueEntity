using System;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Attributes;
using EnTTSharp.Serialization.Binary.AutoRegistration;
using EnTTSharp.Serialization.Xml.AutoRegistration;

namespace RogueEntity.Core.Meta.Items
{
    /// <summary>
    /// Describes an item at a given map or inventory position.
    ///
    /// Type is stored at the highest 1 bit of the 32-bit vector.
    /// If type is 0 it indicates a reference type, the remaining 31 bits
    /// are used as item-id. 
    ///
    /// If type is 1 it indicates simple stateful item where the lower
    /// 16 bits are used as general data storage and the remaining upper
    /// 7 bits are used as item id. 
    /// </summary>
    [EntityKey]
    [EntityXmlSerialization]
    [EntityBinarySerialization]
    public readonly struct ItemReference : IEquatable<ItemReference>, IBulkDataStorageKey<ItemReference>
    {
        public static readonly ItemReference Empty = default;
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
                return raw == 0x8000_0000;
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

                return (ushort)(data & 0xFFFF);
            }
        }

        public byte Age
        {
            get { return (byte)((data & 0x7000_0000) >> 28); }
        }

        public int Key
        {
            get { return (int)(data & 0xFFF_FFFF); }
        }

        public int BulkItemId
        {
            get
            {
                if (IsReference)
                {
                    return 0;
                }

                return (int)((data & 0x7FFF_0000) >> 16);
            }
        }

        public ItemReference WithData(int newData)
        {
            if (IsReference)
            {
                throw new InvalidOperationException("Reference items cannot carry inline data.");
            }

            if (newData < 0 || newData > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(newData), newData, "The given data is not a valid ushort value.");
            }

            return FromBulkItem((short)BulkItemId, (ushort)newData);
        }

        public static ItemReference FromReferencedItem(byte age, int key)
        {
            if (age > MaxAge) throw new ArgumentException();
            if (key < 0 || key > 0x0FFF_FFFF) throw new ArgumentException();

            var rawData = 0x8000_0000;
            rawData |= (uint)(age << 28);
            rawData |= (uint)(key);
            return new ItemReference(rawData);
        }

        public static ItemReference FromBulkItem(short itemId, ushort data)
        {
            if (itemId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(itemId), "should not be zero");
            }

            var rawData = 0u;
            rawData |= (uint)(itemId << 16);
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
            return (int)data;
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
                return $"Item[Ref]{Age:X2}[{Key:X8}]";
            }

            return $"Item[Bulk]{BulkItemId:X4}[{Data:X4}]";
        }

        public static ItemReference BulkItemFactoryMethod(int declarationIndex)
        {
            if (declarationIndex < 1 || declarationIndex > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(declarationIndex), declarationIndex, "should be between 0 and 65,535");
            }

            return FromBulkItem((short)declarationIndex, 0);
        }
    }
}