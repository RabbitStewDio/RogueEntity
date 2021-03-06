using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Movement.GridMovement
{
    [EntityComponent]
    [DataContract]
    [MessagePackObject]
    public readonly struct MovementIntent : IEquatable<MovementIntent>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly TimeSpan StartTime;

        [DataMember(Order = 1)]
        [Key(1)]
        public readonly double DurationInSeconds;

        [DataMember(Order = 2)]
        [Key(2)]
        public readonly Position Origin;

        [DataMember(Order = 3)]
        [Key(3)]
        public readonly Position Target;

        [SerializationConstructor]
        public MovementIntent(TimeSpan startTime, double durationInSeconds, Position origin, Position target)
        {
            StartTime = startTime;
            DurationInSeconds = durationInSeconds;
            Origin = origin;
            Target = target;
        }

        public double AsLerpFactor(TimeSpan currentTime)
        {
            var dx = currentTime - StartTime;
            return (dx.TotalSeconds / DurationInSeconds).Clamp(0, 1);
        }

        public bool Equals(MovementIntent other)
        {
            return StartTime.Equals(other.StartTime) && DurationInSeconds.Equals(other.DurationInSeconds) && Origin.Equals(other.Origin) && Target.Equals(other.Target);
        }

        public override bool Equals(object obj)
        {
            return obj is MovementIntent other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StartTime.GetHashCode();
                hashCode = (hashCode * 397) ^ DurationInSeconds.GetHashCode();
                hashCode = (hashCode * 397) ^ Origin.GetHashCode();
                hashCode = (hashCode * 397) ^ Target.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MovementIntent left, MovementIntent right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementIntent left, MovementIntent right)
        {
            return !left.Equals(right);
        }
    }
}
