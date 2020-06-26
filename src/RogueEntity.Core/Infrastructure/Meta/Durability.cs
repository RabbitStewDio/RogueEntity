using System;
using EnTTSharp.Annotations;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Meta
{
    /// <summary>
    ///    Current item status and health. Items can have status effects, which
    ///    will affect any character interacting with the item. (Poisoned, Burning etc).
    /// </summary>
    [EntityComponent]
    public readonly struct Durability
    {
        public readonly ushort HitPoints;
        public readonly ushort MaxHitPoints;

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