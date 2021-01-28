using RogueEntity.Core.Positioning;
using System;

namespace RogueEntity.Core.Chunks
{
    public readonly struct DynamicViewChunkId : IEquatable<DynamicViewChunkId>
    {
        public readonly int Z;
        public readonly Position TileId;

        public DynamicViewChunkId(int z, Position tileId)
        {
            Z = z;
            TileId = tileId;
        }

        public bool Equals(DynamicViewChunkId other)
        {
            return Z == other.Z && TileId.Equals(other.TileId);
        }

        public override bool Equals(object obj)
        {
            return obj is DynamicViewChunkId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Z * 397) ^ TileId.GetHashCode();
            }
        }

        public static bool operator ==(DynamicViewChunkId left, DynamicViewChunkId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DynamicViewChunkId left, DynamicViewChunkId right)
        {
            return !left.Equals(right);
        }
    }
}
