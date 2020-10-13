using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    /// <summary>
    ///   Contains a quickly accessible state map of all sense related properties of a cell.
    ///   The sense properties here define how a given cell affects the transportation of
    ///   sense information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct SenseProperties : IEquatable<SenseProperties>
    {
        public readonly Percentage blocksLight;
        public readonly Percentage blocksSound;
        public readonly Percentage blocksHeat;
        readonly byte reserved;

        public SenseProperties(Percentage blocksLight,
                               Percentage blocksSound,
                               Percentage blocksHeat)
        {
            this.blocksLight = blocksLight;
            this.blocksSound = blocksSound;
            this.blocksHeat = blocksHeat;
            reserved = 0;
        }

        public override string ToString()
        {
            return $"{nameof(blocksLight)}: {blocksLight}, {nameof(blocksSound)}: {blocksSound}, {nameof(blocksHeat)}: {blocksHeat}";
        }

        public bool Equals(SenseProperties other)
        {
            return blocksLight.Equals(other.blocksLight)
                   && blocksSound.Equals(other.blocksSound)
                   && blocksHeat.Equals(other.blocksHeat);
        }

        public override bool Equals(object obj)
        {
            return obj is SenseProperties other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = blocksLight.GetHashCode();
                hashCode = (hashCode * 397) ^ blocksSound.GetHashCode();
                hashCode = (hashCode * 397) ^ blocksHeat.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SenseProperties left, SenseProperties right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SenseProperties left, SenseProperties right)
        {
            return !left.Equals(right);
        }

        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public SenseProperties WithSensoryResistance(Percentage blocksLight,
                                                     Percentage blocksSound,
                                                     Percentage blocksHeat)
        {
            return new SenseProperties(blocksLight, blocksSound,
                                       blocksHeat);
        }
    }
}