using System;

namespace RogueEntity.Physics2D.Shapes;

public class BoxPhysicsShape2D: IPhysicsShape2D, IEquatable<BoxPhysicsShape2D>
{
    public float Weight { get; }
    public float Width { get; }
    public float Height { get; }

    public BoxPhysicsShape2D(float weight, float width, float height)
    {
        Weight = weight;
        Width = width;
        Height = height;
    }

    public bool Equals(IPhysicsShape2D? other)
    {
        if (other is BoxPhysicsShape2D body)
        {
            return Equals(body);
        }

        return false;
    }

    public bool Equals(BoxPhysicsShape2D? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Weight.Equals(other.Weight) && Width.Equals(other.Width) && Height.Equals(other.Height);
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

        return Equals((BoxPhysicsShape2D)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Weight.GetHashCode();
            hashCode = (hashCode * 397) ^ Width.GetHashCode();
            hashCode = (hashCode * 397) ^ Height.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(BoxPhysicsShape2D? left, BoxPhysicsShape2D? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BoxPhysicsShape2D? left, BoxPhysicsShape2D? right)
    {
        return !Equals(left, right);
    }

    public PhysicsAABB Bounds => new PhysicsAABB(0, 0, Width / 2f, Height / 2f);
}