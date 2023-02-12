using System;

namespace RogueEntity.Physics2D;

public readonly struct Rotation : IEquatable<Rotation>
{
    readonly float rotation;

    Rotation(float rotation)
    {
        this.rotation = rotation;
    }
    
    public float toRadians()
    {
        return rotation;
    }

    public float toAngle360()
    {
        return rotation;
    }

    public static implicit operator float(Rotation r)
    {
        return r.rotation;
    }

    public static Rotation operator +(Rotation a) => a;
    public static Rotation operator -(Rotation a) => new Rotation(-a.rotation);

    public static Rotation operator +(Rotation a, Rotation b)
        => fromRadian(a.rotation + b.rotation);

    public static Rotation operator -(Rotation a, Rotation b)
        => fromRadian(a.rotation - b.rotation);

    public static Rotation operator *(Rotation a, Rotation b)
        => fromRadian(a.rotation * b.rotation);

    public static Rotation operator /(Rotation a, Rotation b)
    {
        if (b.rotation == 0)
        {
            throw new DivideByZeroException();
        }

        return fromRadian(a.rotation / b.rotation);
    }
    
    public static Rotation operator +(float a, Rotation b)
        => fromRadian(a + b.rotation);

    public static Rotation operator -(float a, Rotation b)
        => fromRadian(a - b.rotation);

    public static Rotation operator *(float a, Rotation b)
        => fromRadian(a * b.rotation);

    public static Rotation operator /(float a, Rotation b)
    {
        if (b.rotation == 0)
        {
            throw new DivideByZeroException();
        }

        return fromRadian(a / b.rotation);
    }
    
    public static Rotation operator +(Rotation a, float b)
        => fromRadian(a.rotation + b);

    public static Rotation operator -(Rotation a, float b)
        => fromRadian(a.rotation - b);

    public static Rotation operator *(Rotation a, float b)
        => fromRadian(a.rotation * b);

    public static Rotation operator /(Rotation a, float b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException();
        }

        return fromRadian(a.rotation / b);
    }
    
    public static Rotation fromAngle(float angle)
    {
        if (Single.IsInfinity(angle) || Single.IsNegativeInfinity(angle))
        {
            throw new ArgumentException();
        }

        return new Rotation(Wrap01(angle) * 2 * MathF.PI);
    }

    public static Rotation fromAngle360(float angle)
    {
        if (Single.IsInfinity(angle) || Single.IsNegativeInfinity(angle))
        {
            throw new ArgumentException();
        }

        return new Rotation(Wrap01(angle / 360f) * 2 * MathF.PI);
    }

    public static Rotation fromRadian(float angle)
    {
        if (Single.IsInfinity(angle) || Single.IsNegativeInfinity(angle))
        {
            throw new ArgumentException();
        }

        return new Rotation(angle);
    }

    static float Wrap01(float x)
    {
        return (x % 1 + 1) % 1;
    }

    public bool Equals(Rotation other)
    {
        return rotation.Equals(other.rotation);
    }

    public override bool Equals(object? obj)
    {
        return obj is Rotation other && Equals(other);
    }

    public override int GetHashCode()
    {
        return rotation.GetHashCode();
    }

    public static bool operator ==(Rotation left, Rotation right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rotation left, Rotation right)
    {
        return !left.Equals(right);
    }
}