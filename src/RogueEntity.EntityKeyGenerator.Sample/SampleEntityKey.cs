

using System;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Attributes;
using EnTTSharp.Serialization.Binary.AutoRegistration;
using EnTTSharp.Serialization.Xml.AutoRegistration;
using RogueEntity.Api.ItemTraits;

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------










namespace RogueEntity.EntityKeyGenerator.Sample
{
    [EntityKey]
    [EntityXmlSerialization]
    [EntityBinarySerialization]
    public readonly struct SampleEntityKey : IEquatable<SampleEntityKey>, IBulkDataStorageKey<SampleEntityKey>
    {
        public static readonly SampleEntityKey Empty = default;
        public static int MaxAge => 7;
        readonly uint data;

        SampleEntityKey(uint data)
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

        public SampleEntityKey WithData(int newData)
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

        public static SampleEntityKey FromReferencedItem(byte age, int key)
        {
            if (age > MaxAge) throw new ArgumentException();
            if (key < 0 || key > 0x0FFF_FFFF) throw new ArgumentException();

            var rawData = 0x8000_0000;
            rawData |= (uint)(age << 28);
            rawData |= (uint)(key);
            return new SampleEntityKey(rawData);
        }

        public static SampleEntityKey FromBulkItem(short itemId, ushort data)
        {
            if (itemId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(itemId), "should not be zero");
            }

            var rawData = 0u;
            rawData |= (uint)(itemId << 16);
            rawData |= data;
            return new SampleEntityKey(rawData);
        }

        public bool Equals(SampleEntityKey other)
        {
            return data == other.data;
        }

        public bool Equals(IEntityKey obj)
        {
            return obj is SampleEntityKey other && Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is SampleEntityKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)data;
        }

        public static bool operator ==(SampleEntityKey left, SampleEntityKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SampleEntityKey left, SampleEntityKey right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            if (IsReference)
            {
                return $"SampleEntityKey[Ref]{Age:X2}[{Key:X8}]";
            }

            return $"SampleEntityKey[Bulk]{BulkItemId:X4}[{Data:X4}]";
        }

        public static SampleEntityKey BulkItemFactoryMethod(int declarationIndex)
        {
            if (declarationIndex < 1 || declarationIndex > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(declarationIndex), declarationIndex, "should be between 0 and 65,535");
            }

            return FromBulkItem((short)declarationIndex, 0);
        }
    }


    public sealed class SampleEntityKeyMetaData : IBulkDataStorageMetaData<SampleEntityKey>
    {
        public static readonly SampleEntityKeyMetaData Instance = new SampleEntityKeyMetaData();

        public int MaxAge => SampleEntityKey.MaxAge;
        public bool IsSameBulkType(SampleEntityKey a, SampleEntityKey b) => !a.IsEmpty && !a.IsReference && !b.IsReference && a.BulkItemId == b.BulkItemId;
        public bool IsReferenceEntity(in SampleEntityKey targetItem) => targetItem.IsReference;
        
        public SampleEntityKey CreateReferenceKey(byte age, int entityId) => SampleEntityKey.FromReferencedItem(age, entityId);

        public bool TryCreateBulkKey(int id, int data, out SampleEntityKey key)
        {
            key = SampleEntityKey.BulkItemFactoryMethod(id).WithData(data);
            return true;
        }

        public bool TryDeconstructBulkKey(in SampleEntityKey id, out int entityId, out int payload)
        {
            if (id.IsReference)
            {
                entityId = default;
                payload = default;
                return false;
            }
            
            entityId = id.BulkItemId;
            payload = id.Data;
            return true;
        }

    }
}
