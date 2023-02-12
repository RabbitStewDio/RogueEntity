using System;

namespace RogueEntity.Physics2D.Shapes;

public interface IPhysicsShape2D: IEquatable<IPhysicsShape2D>
{
    public float Weight { get; }
    public PhysicsAABB Bounds { get; }
}