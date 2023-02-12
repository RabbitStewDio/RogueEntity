using System;

namespace RogueEntity.Physics2D.Shapes;

public class SpherePhysicsShape2D: IPhysicsShape2D, IEquatable<SpherePhysicsShape2D>
{
    public float Weight { get; }
    public float Radius { get; }

    public SpherePhysicsShape2D(float weight, float radius)
    {
        Weight = weight;
        Radius = Math.Abs(radius);
    }

    public PhysicsAABB Bounds
    {
        get
        {
            return new PhysicsAABB(0, 0, Radius, Radius);
        }
    }

    public bool Equals(IPhysicsShape2D? other)
    {
        if (other is SpherePhysicsShape2D sphere)
        {
            return Equals(other);
        }
        
        return false;
    }

    public bool Equals(SpherePhysicsShape2D? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Weight.Equals(other.Weight) && Radius.Equals(other.Radius);
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

        return Equals((SpherePhysicsShape2D)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Weight.GetHashCode() * 397) ^ Radius.GetHashCode();
        }
    }

    public static bool operator ==(SpherePhysicsShape2D? left, SpherePhysicsShape2D? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SpherePhysicsShape2D? left, SpherePhysicsShape2D? right)
    {
        return !Equals(left, right);
    }
}