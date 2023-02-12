using RogueEntity.Core.Positioning;
using System;
using System.Numerics;
using tainicom.Aether.Physics2D.Dynamics;

namespace RogueEntity.Physics2D.AetherPhysics;

public static class AetherExtensions
{
    public static Vector2 ToNumerics(this tainicom.Aether.Physics2D.Common.Vector2 v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static tainicom.Aether.Physics2D.Common.Vector2 ToAether(this Vector2 v)
    {
        return new tainicom.Aether.Physics2D.Common.Vector2(v.X, v.Y);
    }

    public static tainicom.Aether.Physics2D.Common.Vector2 ToAether(this Position v)
    {
        return new tainicom.Aether.Physics2D.Common.Vector2((float)v.X, (float)v.Y);
    }

    public static BodyType ToAether(this PhysicsBodyType b)
    {
        return b switch
        {
            PhysicsBodyType.Dynamic => BodyType.Dynamic,
            PhysicsBodyType.Sensor => BodyType.Kinematic,
            PhysicsBodyType.Static => BodyType.Static,
            _ => throw new ArgumentException()
        };
    }
}