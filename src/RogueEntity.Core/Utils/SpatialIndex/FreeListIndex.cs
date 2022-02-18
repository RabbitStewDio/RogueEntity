using System;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public readonly struct FreeListIndex : IEquatable<FreeListIndex>
    {
        public static readonly FreeListIndex Empty = default;
        readonly int value;
        
        FreeListIndex(int value)
        {
            if (value < 0) throw new ArgumentException();
            this.value = value + 1;
        }

        public int Value => value - 1;

        public bool IsEmpty => value == 0;

        public bool Equals(FreeListIndex other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return obj is FreeListIndex other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value;
        }

        public static bool operator ==(FreeListIndex left, FreeListIndex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FreeListIndex left, FreeListIndex right)
        {
            return !left.Equals(right);
        }

        public static FreeListIndex operator +(FreeListIndex left, int right)
        {
            if (left.IsEmpty)
            {
                return Empty;
            }
            
            return new FreeListIndex(left.Value + right);
        }

        public static FreeListIndex Of(int i)
        {
            if (i < 0) throw new ArgumentException();
            return new FreeListIndex(i);
        }
    }
}
