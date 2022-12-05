using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.Continuous;

public readonly struct CircularSearchArea<TPosition>: IEnumerable<TPosition>
    where TPosition: struct, IPosition<TPosition>
{
    public readonly TPosition Origin;
    public readonly int Radius;
    public readonly float BodyRadius;
    
    public CircularSearchRadiusEnumerator<TPosition> GetEnumerator() => 
        new CircularSearchRadiusEnumerator<TPosition>(Origin, Radius, BodyRadius);

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator<TPosition> IEnumerable<TPosition>.GetEnumerator()
    {
        return GetEnumerator();
    }

    public CircularSearchArea(TPosition origin, int radius, float bodyRadius)
    {
        Origin = origin;
        Radius = radius;
        BodyRadius = bodyRadius;
    }

    public override string ToString()
    {
        return $"CircularSearchArea({nameof(Origin)}: {Origin}, {nameof(Radius)}: {Radius}, {nameof(BodyRadius)}: {BodyRadius})";
    }
}