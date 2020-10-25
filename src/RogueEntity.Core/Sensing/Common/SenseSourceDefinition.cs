using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using MessagePack;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Sensing.Common
{
    [DataContract]
    [MessagePackObject]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public readonly struct SenseSourceDefinition : IEquatable<SenseSourceDefinition>
    {
        [Key(0)]
        [DataMember(Order = 0)]
        readonly DistanceCalculation distanceCalculation;
        [Key(1)]
        [DataMember(Order = 1)]
        readonly float intensity;
        [Key(2)]
        [DataMember(Order = 2)]
        readonly float angle;
        [Key(3)]
        [DataMember(Order = 3)]
        readonly float span;
        [Key(4)]
        [DataMember(Order = 4)]
        readonly bool enabled;

        [SerializationConstructor]
        public SenseSourceDefinition(DistanceCalculation distanceCalculation,
                                     float intensity,
                                     float angle = 0,
                                     float span = 360, 
                                     bool enabled = true)
        {
            this.distanceCalculation = distanceCalculation;
            this.angle = (float)((angle > 360.0 || angle < 0) ? Math.IEEERemainder(angle, 360.0) : angle);
            this.span = span.Clamp(0, 360);
            this.intensity = intensity;
            this.enabled = enabled;
        }

        /// <summary>
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="DistanceCalculation"/>, such as <see cref="GoRogue.Radius"/>).
        /// </summary>
        [IgnoreMember]
        [IgnoreDataMember]
        public DistanceCalculation DistanceCalculation => distanceCalculation;

        /// <summary>
        /// Whether or not this source is enabled. If a source is disabled, the source does not calculate values and is effectively assumed to be "off".
        /// </summary>
        [IgnoreMember]
        [IgnoreDataMember]
        public bool Enabled => enabled;

        /// <summary>
        /// Whether or not the spreading of values from this source is restricted to an angle and span.
        /// </summary>
        [IgnoreMember]
        [IgnoreDataMember]
        public bool IsAngleRestricted => Span > 359.5f;

        /// <summary>
        /// The starting value of the source to spread.  Defaults to 1.0.
        /// </summary>
        [IgnoreMember]
        [IgnoreDataMember]
        public float Intensity => intensity;

        /// <summary>
        /// If <see cref="IsAngleRestricted"/> is true, the angle in degrees that represents a line from the source's start to
        /// the outermost center point of the cone formed by the source's calculated values.  0 degrees points right.
        /// Otherwise, this will be 0.0 degrees.
        /// </summary>
        [IgnoreMember]
        [IgnoreDataMember]
        public float Angle => angle;

        /// <summary>
        /// If <see cref="IsAngleRestricted"/> is true, the angle in degrees that represents the full arc of the cone formed by
        /// the source's calculated values.  Otherwise, it will be 360 degrees.
        /// </summary>
        [IgnoreMember]
        [IgnoreDataMember]
        public float Span => span;

        public SenseSourceDefinition WithIntensity(float newIntensity)
        {
            return new SenseSourceDefinition(distanceCalculation, newIntensity, angle, span);
        }
        
        public bool Equals(SenseSourceDefinition other)
        {
            return distanceCalculation == other.distanceCalculation && intensity.Equals(other.intensity) && angle.Equals(other.angle) && span.Equals(other.span) && enabled == other.enabled;
        }

        public override bool Equals(object obj)
        {
            return obj is SenseSourceDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)distanceCalculation;
                hashCode = (hashCode * 397) ^ intensity.GetHashCode();
                hashCode = (hashCode * 397) ^ angle.GetHashCode();
                hashCode = (hashCode * 397) ^ span.GetHashCode();
                hashCode = (hashCode * 397) ^ enabled.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SenseSourceDefinition left, SenseSourceDefinition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SenseSourceDefinition left, SenseSourceDefinition right)
        {
            return !left.Equals(right);
        }
    }
}