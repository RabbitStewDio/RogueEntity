using System;
using System.Diagnostics.Contracts;

namespace RogueEntity.Core.Positioning
{
    public interface IPosition<TPosition>: IEquatable<TPosition>
        where TPosition: IPosition<TPosition> 
    {
        double X { get; }
        double Y { get; }
        double Z { get; }

        int GridX { get; }
        int GridY { get; }
        int GridZ { get; }

        byte LayerId { get; }
        bool IsInvalid { get; }

        [Pure]
        TPosition WithPosition(int x, int y);
        
        [Pure]
        TPosition WithPosition(double tx, double ty);
    }
}