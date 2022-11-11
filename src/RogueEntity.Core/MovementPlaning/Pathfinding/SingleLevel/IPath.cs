using RogueEntity.Core.Movement;
using RogueEntity.Core.Positioning;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;

public interface IPath : IReadOnlyList<(Position position, IMovementMode mode)>, IDisposable
{
    public Position Origin { get; }
    public new PathEnumerator GetEnumerator();
}


public struct PathEnumerator : IEnumerator<(Position, IMovementMode)>
{
    readonly IPath path;
    int index;
    Position pos;
    IMovementMode? mode;

    public PathEnumerator(IPath path)
    {
        this.path = path;
        this.index = -1;
        this.mode = null;
        this.pos = path.Origin;
    }

    public void Reset()
    {
        this.pos = path.Origin;
        this.index = -1;
        this.mode = null;
    }

    object IEnumerator.Current
    {
        get { return Current; }
    }

    public (Position, IMovementMode) Current
    {
        get
        {
            if (mode == null)
            {
                throw new InvalidOperationException();
            }

            return (pos, mode);
        }
    }

    public void Dispose()
    {
    }

    public bool MoveNext()
    {
        if (index + 1 < path.Count)
        {
            index += 1;
            var dir = path[index];
            pos = dir.position;
            mode = dir.mode;
            return true;
        }

        pos = default;
        mode = default;
        return false;
    }
}