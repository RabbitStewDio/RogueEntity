using EnTTSharp.Entities.Attributes;
using MessagePack;
using RogueEntity.Core.Utils;
using System;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Positioning;

[EntityComponent]
[DataContract]
[Serializable]
[MessagePackObject]
public readonly struct BodySize : IEquatable<BodySize>
{
    public static BodySize Empty = new BodySize();
    public static BodySize OneByOne = new BodySize(1, 1);
    
    [DataMember]
    [Key(0)]
    public readonly int Width;
    
    [DataMember]
    [Key(1)]
    public readonly int Height;

    public BodySize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public bool Equals(BodySize other)
    {
        return Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        return obj is BodySize other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Width * 397) ^ Height;
        }
    }

    public static bool operator ==(BodySize left, BodySize right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BodySize left, BodySize right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"BodySize({nameof(Width)}: {Width}, {nameof(Height)}: {Height})";
    }

    public BoundingBox ToBoundingBox<TPosition>(TPosition p)
        where TPosition: struct, IPosition<TPosition>
    {
        var dx = Width / 2;
        var dy = Height / 2;
        return BoundingBox.From(p.GridX - dx, p.GridY - dy, Width, Height);
    }

    public Rectangle ToRectangle(Position2D p)
    {
        var dx = Width / 2;
        var dy = Height / 2;
        return new Rectangle(p.X - dx, p.Y - dy, Width, Height);
    }
}