using System;

namespace RogueEntity.Core.Positioning
{
    public interface IPosition
    {
        double X { get; }
        double Y { get; }
        double Z { get; }

        int GridX { get; }
        int GridY { get; }
        int GridZ { get; }

        byte LayerId { get; }
        bool IsInvalid { get; }
    }

    /// <summary>
    ///   A generic data transfer object for position data. Use this if your need to
    ///   work with position data from multiple organisation schemas.
    ///
    ///   This object is not meant for storage.
    /// </summary>
    public readonly struct Position : IEquatable<Position>, IPosition
    {
        public static Position Invalid = default; 
        
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public byte LayerId { get; }

        public bool IsInvalid => LayerId == 0;

        public Position(double x, double y, double z, byte layer)
        {
            X = x;
            Y = y;
            Z = z;
            LayerId = layer;
        }

        public override string ToString()
        {
            return $"Position({nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Z)}: {Z}, {nameof(LayerId)}: {LayerId})";
        }

        public bool Equals(Position other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && LayerId.Equals(other.LayerId);
        }

        public override bool Equals(object obj)
        {
            return obj is Position other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ LayerId.GetHashCode();
                return hashCode;
            }
        }

        public int GridX => (int)(X + 0.5f);
        public int GridY => (int)(Y + 0.5f);
        public int GridZ => (int)(Z + 0.5f);

        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !left.Equals(right);
        }

        public Position Apply<TPosition>(TPosition p) where TPosition : IPosition
        {
            return new Position(p.X, p.Y, p.Z, this.LayerId);
        }

        public Position From(int x, int y) 
        {
            return new Position(x, y, this.Z, this.LayerId);
        }

        public static Position From<TPosition>(TPosition p) where TPosition: IPosition
        {
            if (p.IsInvalid)
            {
                return new Position();
            }

            return new Position(p.X, p.Y, p.Z, p.LayerId);
        }
    }
}