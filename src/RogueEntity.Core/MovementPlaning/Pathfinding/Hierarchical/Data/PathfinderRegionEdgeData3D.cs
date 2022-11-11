using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;

public class PathfinderRegionEdgeData3D
{
    readonly ObjectPool<PathfinderRegionEdgeData2D> dataPool;
    readonly Dictionary<int, PathfinderRegionEdgeData2D> layers;
    readonly Dictionary<DistanceCalculation, List<IMovementMode>> template;

    public PathfinderRegionEdgeData3D(MovementModeEncoding movementEncoding)
    {
        this.template = new Dictionary<DistanceCalculation, List<IMovementMode>>
        {
            { DistanceCalculation.Manhattan, new List<IMovementMode>() },
            { DistanceCalculation.Chebyshev, new List<IMovementMode>() },
            { DistanceCalculation.Euclid, new List<IMovementMode>() }
        };
        this.dataPool = new DefaultObjectPool<PathfinderRegionEdgeData2D>(new PathfinderRegionEdgeData2DPolicy(template, movementEncoding));
        this.layers = new Dictionary<int, PathfinderRegionEdgeData2D>();
    }

    public void Clear()
    {
        foreach (var l in layers)
        {
            l.Value.Clear();
        }
        layers.Clear();
    }

    public BufferList<DistanceCalculation> GetMovementStyles(BufferList<DistanceCalculation>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        foreach (var (distanceCalculation, movementTypes) in template)
        {
            if (movementTypes.Count > 0)
            {
                buffer.Add(distanceCalculation);
            }
        }
        
        return buffer;
    }
    public BufferList<(DistanceCalculation, IMovementMode)> GetMovements(BufferList<(DistanceCalculation, IMovementMode)>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        foreach (var (distanceCalculation, movementModes) in template)
        {
            foreach (var mode in movementModes)
            {
                buffer.Add((distanceCalculation, mode));
            }
        }

        return buffer;
    }
    
    public void RegisterMovement(DistanceCalculation style, IMovementMode mode)
    {
        if (!template.TryGetValue(style, out var modes))
        {
            return;
        }

        // This is a one time operation at the init phase; its ok to be clunky.
        if (!modes.Contains(mode))
        {
            modes.Add(mode);
        }
    }
    
    public BufferList<int> GetActiveLayers(BufferList<int>? retval = null)
    {
        retval = BufferList.PrepareBuffer(retval);
        foreach (var k in layers)
        {
            retval.Add(k.Key);
        }

        return retval;
    }

    public bool TryGetView(int z, [MaybeNullWhen(false)] out PathfinderRegionEdgeData2D view, DataViewCreateMode mode = DataViewCreateMode.Nothing)
    {
        if (layers.TryGetValue(z, out view))
        {
            return true;
        }

        if (mode == DataViewCreateMode.Nothing)
        {
            return false;
        }

        view = dataPool.Get();
        layers[z] = view;
        return true;
    }

    public void ExpireView(int z)
    {
        layers.Remove(z);
    }

    class PathfinderRegionEdgeData2DPolicy : IPooledObjectPolicy<PathfinderRegionEdgeData2D>
    {
        readonly ObjectPool<PathfinderRegionEdgeData> edgeDataPool;

        public PathfinderRegionEdgeData2DPolicy(Dictionary<DistanceCalculation, List<IMovementMode>> template, 
                                                MovementModeEncoding movementModeEncoding)
        {
            edgeDataPool = new DefaultObjectPool<PathfinderRegionEdgeData>(new PathfinderRegionEdgeDataPolicy(template, movementModeEncoding));
        }

        public PathfinderRegionEdgeData2D Create()
        {
            return new PathfinderRegionEdgeData2D(edgeDataPool);
        }

        public bool Return(PathfinderRegionEdgeData2D obj)
        {
            obj.Clear();
            return true;
        }
    }

    class PathfinderRegionEdgeDataPolicy : IPooledObjectPolicy<PathfinderRegionEdgeData>
    {
        readonly ObjectPool<TraversableZonePathData> zonePool;
        readonly Dictionary<DistanceCalculation, List<IMovementMode>> template;
        readonly ObjectPool<Dictionary<DistanceCalculation, Dictionary<IMovementMode, TraversableZonePathData>>> zoneDataByMovementStylePool;
        readonly ObjectPool<Dictionary<IMovementMode, TraversableZonePathData>> zoneDataByMovementModePool;

        public PathfinderRegionEdgeDataPolicy(Dictionary<DistanceCalculation, List<IMovementMode>> template, 
                                              MovementModeEncoding movementModeEncoding)
        {
            this.template = template;
            zonePool = new DefaultObjectPool<TraversableZonePathData>(new TraversableZonePathDataPolicy(movementModeEncoding), 4096);
            zoneDataByMovementModePool =
                new DefaultObjectPool<Dictionary<IMovementMode, TraversableZonePathData>>
                    (new DefaultPooledObjectPolicy<Dictionary<IMovementMode, TraversableZonePathData>>(), 4096);
            zoneDataByMovementStylePool =
                new DefaultObjectPool<Dictionary<DistanceCalculation, Dictionary<IMovementMode, TraversableZonePathData>>>
                    (new DefaultPooledObjectPolicy<Dictionary<DistanceCalculation, Dictionary<IMovementMode, TraversableZonePathData>>>(), 4096);
        }

        public PathfinderRegionEdgeData Create()
        {
            return new PathfinderRegionEdgeData(zonePool, zoneDataByMovementStylePool, zoneDataByMovementModePool, template);
        }

        public bool Return(PathfinderRegionEdgeData obj)
        {
            obj.Clear();
            return true;
        }
    }

    class TraversableZonePathDataPolicy : IPooledObjectPolicy<TraversableZonePathData>
    {
        readonly MovementModeEncoding movementModeEncoding;
        readonly ObjectPool<List<(Direction, byte)>> connectionPool;

        public TraversableZonePathDataPolicy(MovementModeEncoding movementModeEncoding)
        {
            this.movementModeEncoding = movementModeEncoding;
            this.connectionPool = new DefaultObjectPool<List<(Direction, byte)>>(new ListObjectPoolPolicy<(Direction, byte)>());
        }

        public TraversableZonePathData Create()
        {
            return new TraversableZonePathData(movementModeEncoding, connectionPool);
        }

        public bool Return(TraversableZonePathData obj)
        {
            return true;
        }
    }
}