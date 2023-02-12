using EnTTSharp;
using System.Numerics;

namespace RogueEntity.Physics2D;

public readonly struct Contact2D
{
    public readonly Vector2 Normal;
    public readonly Vector2 Position;
    public readonly Optional<Vector2> Force;

    public Contact2D(Vector2 normal, Vector2 position, Optional<Vector2> force)
    {
        Normal = normal;
        Position = position;
        Force = force;
    }
}

public interface IPhysicsBody2D
{
    public bool IsKinematic { get; }
    public Vector2 Position { get; }
    public float Rotation { get; }
    public Vector2 Velocity { get; }
    public float AngularVelocity { get; }
    public bool TryMoveTo(Vector2 position);
    public bool TryRotateTo(float orientation);
}