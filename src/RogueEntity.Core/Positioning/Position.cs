﻿using System;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning
{
    /// <summary>
    ///   A generic data transfer object for position data. Use this if your need to
    ///   work with position data from multiple organisation schemas.
    ///
    ///   This object is not meant for storage.
    /// </summary>
    public readonly struct Position : IEquatable<Position>, IPosition
    {
        public static Position Invalid = default;
        readonly byte layerId;

        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public byte LayerId
        {
            get
            {
                if (layerId == 0) throw new InvalidOperationException();
                return (byte) (layerId - 1);
            }
        }

        public bool IsInvalid => layerId == 0;

        public Position(double x, double y, double z, MapLayer layer) : this(x, y, z, layer.LayerId)
        {
        }

        public Position(double x, double y, double z, byte layer)
        {
            X = x;
            Y = y;
            Z = z;
            layerId = (byte) (layer + 1);
        }

        public override string ToString()
        {
            if (IsInvalid)
            {
                return "Position(Invalid)";
            }
            return $"Position({nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Z)}: {Z}, {nameof(LayerId)}: {LayerId})";
        }

        public bool Equals(Position other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && layerId.Equals(other.layerId);
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
                hashCode = (hashCode * 397) ^ layerId.GetHashCode();
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

        public Position Apply<TPosition>(TPosition p)
            where TPosition : IPosition
        {
            return new Position(p.X, p.Y, p.Z, this.LayerId);
        }

        public Position From(int x, int y)
        {
            return new Position(x, y, this.Z, this.LayerId);
        }

        public Position From(double x, double y)
        {
            return new Position(x, y, this.Z, this.LayerId);
        }

        public static Position From<TPosition>(TPosition p)
            where TPosition : IPosition
        {
            if (p.IsInvalid)
            {
                return new Position();
            }

            return new Position(p.X, p.Y, p.Z, p.LayerId);
        }

        public static Position Of(MapLayer layer, double x, double y, double z = 0)
        {
            return new Position(x, y, z, layer.LayerId);
        }
    }
}