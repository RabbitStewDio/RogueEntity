using System;
using System.Numerics;

namespace RogueEntity.Physics2D.Shapes;

public readonly struct PhysicsShapeChild : IEquatable<PhysicsShapeChild>
{
    public readonly Vector2 Position;
    public readonly Rotation Rotation;
    public readonly IPhysicsShape2D Child;

    public PhysicsShapeChild(Vector2 position, Rotation rotation, IPhysicsShape2D child)
    {
        Position = position;
        Rotation = rotation;
        Child = child;
    }

    public bool Equals(PhysicsShapeChild other)
    {
        return Position.Equals(other.Position) && Rotation.Equals(other.Rotation) && Child.Equals(other.Child);
    }

    public override bool Equals(object? obj)
    {
        return obj is PhysicsShapeChild other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Position.GetHashCode();
            hashCode = (hashCode * 397) ^ Rotation.GetHashCode();
            hashCode = (hashCode * 397) ^ Child.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(PhysicsShapeChild left, PhysicsShapeChild right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PhysicsShapeChild left, PhysicsShapeChild right)
    {
        return !left.Equals(right);
    }
}