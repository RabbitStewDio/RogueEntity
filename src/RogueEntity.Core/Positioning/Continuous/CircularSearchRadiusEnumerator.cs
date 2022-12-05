using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.Continuous;

public struct CircularSearchRadiusEnumerator<TPosition> : IEnumerator<TPosition>
    where TPosition: struct, IPosition<TPosition>
{
    const float Pi2 = (float)(2 * Math.PI); 
    readonly TPosition origin;
    readonly int maxRadius;
    readonly float bodyRadius;
    // Position on the circumference line; zero to one.
    float currentAngle;
    float currentRadius;
    float currentIncrement;

    public CircularSearchRadiusEnumerator(in TPosition origin, int searchRadius, float bodyRadius)
    {
        this.origin = origin;
        this.maxRadius = searchRadius;
        this.bodyRadius = bodyRadius;
        currentAngle = 0;
        currentRadius = -1;
        currentIncrement = 0;
    }

    public bool MoveNext()
    {
        if (currentRadius < 0)
        {
            currentRadius = 0;
            currentAngle = 0;
            currentIncrement = 1;
            return true;
        }

        if ((currentAngle + currentIncrement) < 1)
        {
            currentAngle = currentIncrement / 2;
            currentRadius += bodyRadius / 2;
            currentIncrement = (bodyRadius + (Pi2 * currentRadius)) / (Pi2 * currentRadius);
        }
        else
        {
            currentAngle += currentIncrement;
        }

        if (currentRadius > maxRadius)
        {
            return false;
        }

        return true;
    }

    public void Reset()
    {
        currentAngle = 0;
        currentRadius = 0;
        currentIncrement = 0;
    }

    object IEnumerator.Current
    {
        get { return Current; }
    }

    public void Dispose()
    {
    }

    public TPosition Current
    {
        get
        {
            return default;
        }
    }
}