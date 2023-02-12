using RogueEntity.Core.Utils;
using System;
using System.Numerics;

namespace RogueEntity.Physics2D;

public readonly struct PhysicsAABB
{
    readonly float centerX;
    readonly float centerY;
    readonly float extendX;
    readonly float extendY;

    public Vector2 Center => new Vector2(centerX, centerY);
    
    public PhysicsAABB(float centerX, float centerY, float extendX, float extendY)
    {
        this.centerX = centerX;
        this.centerY = centerY;
        this.extendX = extendX;
        this.extendY = extendY;
    }

    public PhysicsAABB Rotate(float angle)
    {
        var rot = Matrix3x2.CreateRotation(angle * 2 * MathF.PI);
        var a1 = Vector2.Transform(new Vector2(centerX + extendX, centerY + extendY), rot);
        var a2 = Vector2.Transform(new Vector2(centerX + extendX, centerY - extendY), rot);
        var a3 = Vector2.Transform(new Vector2(centerX - extendX, centerY + extendY), rot);
        var a4 = Vector2.Transform(new Vector2(centerX - extendX, centerY - extendY), rot);
        var minX = Math.Min(Math.Min(a1.X, a2.X), Math.Min(a3.X, a4.X));
        var minY = Math.Min(Math.Min(a1.Y, a2.Y), Math.Min(a3.Y, a4.Y));
        var maxX = Math.Max(Math.Max(a1.X, a2.X), Math.Max(a3.X, a4.X));
        var maxY = Math.Max(Math.Max(a1.Y, a2.Y), Math.Max(a3.Y, a4.Y));
        return new PhysicsAABB(centerX, centerY,
                               Math.Max(Math.Abs(minX), Math.Abs(maxX)),
                               Math.Max(Math.Abs(minY), Math.Abs(maxY)));
    }

    public PhysicsAABB Translate(Vector2 p)
    {
        return new PhysicsAABB(p.X + centerX, p.Y + centerY, extendX, extendY);
    }

    public PhysicsAABB Union(PhysicsAABB childBounds)
    {
        var minX1 = MathF.Min(childBounds.centerX - childBounds.extendX, centerX - extendX);
        var maxX1 = MathF.Max(childBounds.centerX + childBounds.extendX, centerX + extendX);
        var minY1 = MathF.Min(childBounds.centerY - childBounds.extendY, centerY - extendY);
        var maxY1 = MathF.Max(childBounds.centerY + childBounds.extendY, centerY + extendY);

        
        var dx = (maxX1 - minX1);
        var dy = (maxY1 - minY1);
        var cx = dx / 2;
        var cy = dy / 2;
        return new PhysicsAABB(cx, cy, maxX1 - cx, maxY1 - cy);
    }

    public BoundingBox ToBoundingBox()
    {
        var rect = new Rectangle((int)MathF.Floor(centerX - extendX),
                                 (int)MathF.Floor(centerY - extendY),
                                 (int)MathF.Ceiling(extendX * 2),
                                 (int)MathF.Ceiling(extendY * 2));
        return BoundingBox.From(rect);
    }
}