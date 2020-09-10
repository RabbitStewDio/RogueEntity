using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Meta.ItemTraits
{
    /// <summary>
    ///    Current item status and health. Items can have status effects, which
    ///    will affect any character interacting with the item. (Poisoned, Burning etc).
    /// </summary>
    [EntityComponent(EntityConstructor.NonConstructable)]
    [MessagePackObject]
    [DataContract]
    public readonly struct Durability
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly ushort HitPoints;
        [DataMember(Order = 1)]
        [Key(1)]
        public readonly ushort MaxHitPoints;

        [SerializationConstructor]
        public Durability(int hitPoints, ushort maxHitPoints)
        {
            HitPoints = (ushort) hitPoints.Clamp(0, maxHitPoints);
            MaxHitPoints = Math.Max((ushort)1, maxHitPoints);
        }

        public Durability WithHitPoints(int hp)
        {
            return new Durability(hp, MaxHitPoints);
        }

        public Durability WithAppliedDamage(int hp)
        {
            if (hp >= HitPoints)
            {
                return new Durability(0, MaxHitPoints);
            }
            return new Durability((ushort)(HitPoints - hp), MaxHitPoints);
        }

        public override string ToString()
        {
            return $"Durability({HitPoints} / {MaxHitPoints})";
        }
    }
}