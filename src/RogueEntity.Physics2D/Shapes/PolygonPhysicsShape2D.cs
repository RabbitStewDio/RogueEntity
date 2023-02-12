using RogueEntity.Api.Utils;
using System;
using System.Numerics;

namespace RogueEntity.Physics2D.Shapes;

public class PolygonPhysicsShape2D: IPhysicsShape2D, IEquatable<PolygonPhysicsShape2D>
{
    public float Weight { get; }
    public ReadOnlyListWrapper<Vector2> Points { get; }

    public PolygonPhysicsShape2D(float weight, ReadOnlyListWrapper<Vector2> points)
    {
        Weight = weight;
        Points = points;
    }

    public PhysicsAABB Bounds
    {
        get
        {
            if (Points.Count == 0) return default;
            
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            foreach (var p in Points)
            {
                minX = MathF.Min(p.X, minX);
                minY = MathF.Min(p.Y, minY);
                maxX = MathF.Max(p.X, maxX);
                maxY = MathF.Max(p.Y, maxY);
            }

            var dx = maxX - minX;
            var dy = maxY - minY;
            var cx = dx / 2;
            var cy = dy / 2;
            return new PhysicsAABB(cx, cy, maxX - cx, maxY - cy);
        }
    }

    public bool Equals(IPhysicsShape2D? other)
    {
        if (other is PolygonPhysicsShape2D body)
        {
            return Equals(body);
        }

        return false;
    }

    public bool Equals(PolygonPhysicsShape2D? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Weight.Equals(other.Weight) && Points.Equals(other.Points);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((PolygonPhysicsShape2D)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Weight.GetHashCode() * 397) ^ Points.GetHashCode();
        }
    }

    public static bool operator ==(PolygonPhysicsShape2D? left, PolygonPhysicsShape2D? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PolygonPhysicsShape2D? left, PolygonPhysicsShape2D? right)
    {
        return !Equals(left, right);
    }
}