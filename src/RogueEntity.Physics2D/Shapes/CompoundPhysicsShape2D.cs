using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using System;
using System.Collections.Generic;

namespace RogueEntity.Physics2D.Shapes;

public class CompoundPhysicsShape2D: IPhysicsShape2D, IEquatable<CompoundPhysicsShape2D>
{
    public float Weight { get; }
    readonly List<PhysicsShapeChild> children;

    public CompoundPhysicsShape2D(float weight, IReadOnlyList<PhysicsShapeChild> children)
    {
        Weight = weight;
        this.children = new List<PhysicsShapeChild>();
        foreach (var c in children)
        {
            this.children.Add(c);
        }
    }

    public ReadOnlyListWrapper<PhysicsShapeChild> Children => children;

    public bool Equals(IPhysicsShape2D other)
    {
        if (other is CompoundPhysicsShape2D body)
        {
            return Equals(body);
        }

        return false;
    }

    public PhysicsAABB Bounds
    {
        get
        {
            if (children.Count == 0)
            {
                return default;
            }

            var first = true;
            var result = new PhysicsAABB();
            foreach (var c in children)
            {
                var childBounds = c.Child.Bounds.Rotate(c.Rotation).Translate(c.Position);
                if (first)
                {
                    result = childBounds;
                    first = false;
                }
                else
                {
                    result = result.Union(childBounds);
                }
            }

            return result;
        }
    }

    public bool Equals(CompoundPhysicsShape2D? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return children.EqualsList(other.children) && Weight.Equals(other.Weight);
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

        return Equals((CompoundPhysicsShape2D)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hc = Weight.GetHashCode();
            hc = hc * 397 ^ children.Count;
            foreach (var c in children)
            {
                hc = hc * 397 ^ c.GetHashCode();
            }
            
            return hc;
        }
    }

    public static bool operator ==(CompoundPhysicsShape2D? left, CompoundPhysicsShape2D? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(CompoundPhysicsShape2D? left, CompoundPhysicsShape2D? right)
    {
        return !Equals(left, right);
    }
}