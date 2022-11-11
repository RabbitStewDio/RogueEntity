using RogueEntity.Core.Movement;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;

public class SingleLevelPath : IPath
{
    readonly List<(Direction direction, IMovementMode mode)> steps;
    readonly SingleLevelPathPool pool;
    int zLevel;
    Position2D origin;
    bool disposed;

    public SingleLevelPath(SingleLevelPathPool pool)
    {
        this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
        this.steps = new List<(Direction, IMovementMode)>();
    }

    public int Count => steps.Count;

    public (Position, IMovementMode) this[int index]
    {
        get
        {
            if (index < 0 || index >= steps.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            Position2D p = origin;
            IMovementMode? mode = null;
            for (int step = 0; step <= index; step += 1)
            {
                var (d, stepMode) = steps[step];
                mode = stepMode;
                p += d;
            }

            var pos = new Position(p.X, p.Y, zLevel, MapLayer.Indeterminate);
            return (pos, mode!);
        }
    }

    public void Init()
    {
        disposed = false;
    }

    public void BeginRecordPath(Position2D pos, int z)
    {
        steps.Clear();
        origin = pos;
        zLevel = z;
    }

    public void RecordStep(Direction posChange, IMovementMode mode)
    {
        if (mode == null)
        {
            throw new ArgumentNullException(nameof(mode));
        }

        steps.Add((posChange, mode));
    }
    
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        
        pool.Return(this);
        disposed = true;
    }
    
    public Position Origin => new Position(origin.X, origin.Y, zLevel, MapLayer.Indeterminate);

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