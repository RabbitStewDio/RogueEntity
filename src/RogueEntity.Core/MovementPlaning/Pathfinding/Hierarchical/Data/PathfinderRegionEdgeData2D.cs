using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public class PathfinderRegionEdgeData2D
{
    readonly ObjectPool<PathfinderRegionEdgeData> tilePool;
    readonly Dictionary<GridPosition2D, PathfinderRegionEdgeData> tiles;
    PathfinderRegionEdgeDataState state;

    public PathfinderRegionEdgeData2D(ObjectPool<PathfinderRegionEdgeData> tilePool)
    {
        this.tilePool = tilePool ?? throw new ArgumentNullException(nameof(tilePool));
        this.tiles = new Dictionary<GridPosition2D, PathfinderRegionEdgeData>();
    }

    public BufferList<PathfinderRegionEdgeData> GetActiveTiles(BufferList<PathfinderRegionEdgeData>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        foreach (var (_, v) in tiles)
        {
            buffer.Add(v);
        }

        return buffer;
    }

    public bool TryGetView(GridPosition2D pos, [MaybeNullWhen(false)] out PathfinderRegionEdgeData view, DataViewCreateMode mode = DataViewCreateMode.Nothing)
    {
        if (tiles.TryGetValue(pos, out view))
        {
            return true;
        }

        if (mode == DataViewCreateMode.Nothing)
        {
            return false;
        }

        view = tilePool.Get();
        view.Init(pos);
        tiles[pos] = view;
        return true;
    }

    public BufferList<TraversableZonePathData> GetZoneData(GlobalTraversableZoneId zone, BufferList<TraversableZonePathData>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        if (!TryGetView(zone.RegionId, out var view))
        {
            return buffer;
        }

        return view.GetZoneData(zone, buffer);
    }

    public PathfinderRegionEdgeDataState State => state;

    public void MarkRemoved()
    {
        state = PathfinderRegionEdgeDataState.RemoveScheduled;
    }

    public void MarkModified()
    {
        state = PathfinderRegionEdgeDataState.Modified;
    }

    public void MarkClean()
    {
        state = PathfinderRegionEdgeDataState.Clean;
    }
    
    public void Clear()
    {
        foreach (var t in tiles)
        {
            tilePool.Return(t.Value);
        }

        tiles.Clear();
        state = PathfinderRegionEdgeDataState.Modified;
    }

    public void RemoveView(int x, int y)
    {
        this.tiles.Remove(new GridPosition2D(x, y));
    }
    
    public void AddInboundEdge(DistanceCalculation style, IMovementMode mode, 
                               PathfinderRegionEdge edge)
    {
        if (TryGetView(edge.TargetZone.RegionId, out var zone))
        {
            zone.AddInboundEdge(style, mode, edge);
        }
    }
}