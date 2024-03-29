﻿using System;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Api.ItemTraits
{
    [MessagePackObject]
    [DataContract]
    public readonly struct ItemDeclarationId : IEquatable<ItemDeclarationId>
    {
        public static readonly ItemDeclarationId Empty = default; 
        
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly string Id;

        [IgnoreDataMember]
        [IgnoreMember]
        public bool IsInvalid => Id == null;
        
        [SerializationConstructor]
        public ItemDeclarationId(string id)
        {
            this.Id = id;
        }

        public bool Equals(ItemDeclarationId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemDeclarationId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public static bool operator ==(ItemDeclarationId left, ItemDeclarationId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemDeclarationId left, ItemDeclarationId right)
        {
            return !left.Equals(right);
        }

        public static implicit operator ItemDeclarationId(string s)
        {
            return new ItemDeclarationId(s);
        }

        public override string ToString()
        {
            return $"ItemDeclarationId('{Id}')";
        }
    }
}