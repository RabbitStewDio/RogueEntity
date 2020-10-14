using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using GoRogue;
using GoRogue.SenseMapping;
using MessagePack;
using RogueEntity.Core.Utils;

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
        readonly float radius;
        [Key(2)]
        [DataMember(Order = 2)]
        readonly float intensity;
        [Key(3)]
        [DataMember(Order = 3)]
        readonly float angle;
        [Key(4)]
        [DataMember(Order = 4)]
        readonly float span;
        [Key(5)]
        [DataMember(Order = 5)]
        readonly bool enabled;

        public SenseSourceDefinition(DistanceCalculation distanceCalculation,
                                     float radius,
                                     float intensity): this(distanceCalculation, radius, intensity, 0, 360)
        {
        }

        [SerializationConstructor]
        public SenseSourceDefinition(DistanceCalculation distanceCalculation,
                                     float radius,
                                     float intensity,
                                     float angle,
                                     float span)
        {
            this.distanceCalculation = distanceCalculation;
            this.angle = (float)((angle > 360.0 || angle < 0) ? Math.IEEERemainder(angle, 360.0) : angle);
            this.radius = Math.Max(1, radius);
            this.span = span.Clamp(0, 360);
            this.intensity = intensity;
            this.enabled = true;
        }

        /// <summary>
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="GoRogue.DistanceCalculation"/>, such as <see cref="GoRogue.Radius"/>).
        /// </summary>
        [IgnoreMember]
        [IgnoreDataMember]
        public DistanceCalculation DistanceCalculation => distanceCalculation;

        /// <summary>
        /// Whether or not this source is enabled. If a source is disabled when <see cref="SenseMap{TSenseSource}.Calculate"/>
        /// is called, the source does not calculate values and is effectively assumed to be "off".
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

        /// <summary>
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed. Changing this will trigger
        /// resizing (re-allocation) of the underlying arrays.
        /// </summary>
        [IgnoreMember]
        [IgnoreDataMember]
        public float Radius => radius;

        public SenseSourceDefinition WithIntensity(float intensity, float radius)
        {
            return new SenseSourceDefinition(distanceCalculation, radius, intensity, angle, span);
        }
        
        public bool Equals(SenseSourceDefinition other)
        {
            return Equals(distanceCalculation, other.distanceCalculation) && radius.Equals(other.radius) && intensity.Equals(other.intensity) && angle.Equals(other.angle) && span.Equals(other.span) && enabled == other.enabled;
        }

        public override bool Equals(object obj)
        {
            return obj is SenseSourceDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (distanceCalculation != null ? distanceCalculation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ radius.GetHashCode();
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