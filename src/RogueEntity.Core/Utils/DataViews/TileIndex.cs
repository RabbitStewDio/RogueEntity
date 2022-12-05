using System;

namespace RogueEntity.Core.Utils.DataViews;

public readonly struct TileIndex : IEquatable<TileIndex>
{
    public readonly int X;
    public readonly int Y;

    public TileIndex(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    public bool Equals(TileIndex other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        return obj is TileIndex other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

    public static bool operator ==(TileIndex left, TileIndex right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TileIndex left, TileIndex right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"{nameof(TileIndex)}({nameof(X)}: {X}, {nameof(Y)}: {Y})";
    }
}