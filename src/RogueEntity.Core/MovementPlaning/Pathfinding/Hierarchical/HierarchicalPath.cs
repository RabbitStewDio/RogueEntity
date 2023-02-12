using RogueEntity.Core.Movement;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

public class HierarchicalPath: IPath
{
    readonly List<IPath> pathSegments;
    public int Count { get; private set; }

    public HierarchicalPath()
    {
        pathSegments = new List<IPath>();
    }

    public (Position, IMovementMode) this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            int segmentOffset = 0;
            for (int p = 0; p < pathSegments.Count; p += 1)
            {
                var ps = pathSegments[p];
                if (index >= (ps.Count + segmentOffset))
                {
                    segmentOffset += ps.Count;
                    continue;
                }

                var idx = index - segmentOffset;
                return ps[idx];
            }

            throw new IndexOutOfRangeException();
        }
    }

    public void Dispose()
    {
        foreach (var p in pathSegments)
        {
            p.Dispose();
        }
        pathSegments.Clear();
        Count = 0;
    }

    public void BeginRecordPath(GridPosition2D pos, int z)
    {
        pathSegments.Clear();
        Count = 0;
        Origin = new Position(pos.X, pos.Y, z, MapLayer.Indeterminate);
    }

    public void AddSegment(IPath path)
    {
        pathSegments.Add(path);
        Count += path.Count;
    }
    
    public Position Origin { get; private set; }
    
    public PathEnumerator GetEnumerator()
    {
        return new PathEnumerator(this);
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    IEnumerator<(Position position, IMovementMode mode)> IEnumerable<(Position position, IMovementMode mode)>.GetEnumerator()
    {
        return GetEnumerator();
    }

}