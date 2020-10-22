using System;

namespace RogueEntity.Core.Movement.Maps
{
    /// <summary>
    ///   MovementAllowedData encodes grid connectivity as flags. Each direction
    ///   is encoded as one bit of the struct's data.
    /// </summary>
    public readonly struct MovementAllowedData : IEquatable<MovementAllowedData>
    {
        public static readonly MovementAllowedData Blocked = new MovementAllowedData(0);
        public static readonly MovementAllowedData Free = new MovementAllowedData(255);
        readonly byte backend;

        public MovementAllowedData(byte backend)
        {
            this.backend = backend;
        }

        public bool this[MovementDirection d]
        {
            get
            {
                var f = (byte)d;
                return (backend & f) == f;
            }
        }

        public MovementAllowedData With(MovementDirection d)
        {
            var f = (byte)d;
            return new MovementAllowedData((byte) (backend | f));
        }

        public MovementAllowedData Without(MovementDirection d)
        {
            var f = (byte)d;
            var b = backend & (~f);
            return new MovementAllowedData((byte) b);
        }

        public MovementAllowedData Combined(MovementAllowedData b)
        {
            return new MovementAllowedData((byte) (backend & b.backend));
        }

        public override string ToString()
        {
            return $"{nameof(backend)}: {Convert.ToString(backend, 2)}";
        }

        public bool Equals(MovementAllowedData other)
        {
            return backend == other.backend;
        }

        public override bool Equals(object obj)
        {
            return obj is MovementAllowedData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return backend.GetHashCode();
        }

        public static bool operator ==(MovementAllowedData left, MovementAllowedData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MovementAllowedData left, MovementAllowedData right)
        {
            return !left.Equals(right);
        }
    }
}