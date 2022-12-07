﻿

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










namespace RogueEntity.Core.Meta.EntityKeys
{
    [EntityKey]
    [EntityXmlSerialization]
    [EntityBinarySerialization]
    [EntityKeyMetaData(typeof(ProjectileReferenceMetaData))]
    public readonly struct ProjectileReference : IEquatable<ProjectileReference>, IBulkDataStorageKey<ProjectileReference>
    {
        public static readonly ProjectileReference Empty = default;
        public static int MaxAge => 7;
        readonly uint data;

        ProjectileReference(uint data)
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

        public ProjectileReference WithData(int newData)
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

        public static ProjectileReference FromReferencedItem(byte age, int key)
        {
            if (age > MaxAge) throw new ArgumentException();
            if (key < 0 || key > 0x0FFF_FFFF) throw new ArgumentException();

            var rawData = 0x8000_0000;
            rawData |= (uint)(age << 28);
            rawData |= (uint)(key);
            return new ProjectileReference(rawData);
        }

        public static ProjectileReference FromBulkItem(short itemId, ushort data)
        {
            if (itemId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(itemId), "should not be zero");
            }

            var rawData = 0u;
            rawData |= (uint)(itemId << 16);
            rawData |= data;
            return new ProjectileReference(rawData);
        }

        public bool Equals(ProjectileReference other)
        {
            return data == other.data;
        }

        public bool Equals(IEntityKey obj)
        {
            return obj is ProjectileReference other && Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectileReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)data;
        }

        public static bool operator ==(ProjectileReference left, ProjectileReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ProjectileReference left, ProjectileReference right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return $"ProjectileReference[Empty]";
            }

            if (IsReference)
            {
                return $"ProjectileReference[Ref]{Age:X2}[{Key:X8}]";
            }

            return $"ProjectileReference[Bulk]{BulkItemId:X4}[{Data:X4}]";
        }

        public static ProjectileReference BulkItemFactoryMethod(int declarationIndex)
        {
            if (declarationIndex < 1 || declarationIndex > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(declarationIndex), declarationIndex, "should be between 0 and 65,535");
            }

            return FromBulkItem((short)declarationIndex, 0);
        }
    }


    public sealed class ProjectileReferenceMetaData : IBulkDataStorageMetaData<ProjectileReference>
    {
        public static readonly ProjectileReferenceMetaData Instance = new ProjectileReferenceMetaData();

        public int MaxAge => ProjectileReference.MaxAge;
        public int MaxBulkKeyTypes => 0x7FFE;
        public bool IsReferenceEntity(in ProjectileReference targetItem) => targetItem.IsReference;
        
        public ProjectileReference CreateReferenceKey(byte age, int entityId) => ProjectileReference.FromReferencedItem(age, entityId);

        public bool TryCreateBulkKey(int id, int data, out ProjectileReference key)
        {
            key = ProjectileReference.BulkItemFactoryMethod(id).WithData(data);
            return true;
        }

        public bool TryDeconstructBulkKey(in ProjectileReference id, out int entityId, out int payload)
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
