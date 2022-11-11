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

public class PathfinderRegionEdgeData
{
    readonly ObjectPool<Dictionary<DistanceCalculation, Dictionary<IMovementMode, TraversableZonePathData>>> zoneDataByMovementStylePool;
    readonly ObjectPool<Dictionary<IMovementMode, TraversableZonePathData>> zoneDataByMovementModePool;
    readonly Dictionary<TraversableZoneId, Dictionary<DistanceCalculation, Dictionary<IMovementMode, TraversableZonePathData>>> data;
    readonly Dictionary<DistanceCalculation, List<IMovementMode>> template;
    readonly ObjectPool<TraversableZonePathData> dataPool;
    readonly Dictionary<TraversableZoneId, bool> zoneState;
    
    PathfinderRegionEdgeDataState state;
    Position2D regionId;

    public PathfinderRegionEdgeData(ObjectPool<TraversableZonePathData> dataPool,
                                    ObjectPool<Dictionary<DistanceCalculation, Dictionary<IMovementMode, TraversableZonePathData>>> zoneDataByMovementStylePool,
                                    ObjectPool<Dictionary<IMovementMode, TraversableZonePathData>> zoneDataByMovementModePool,
                                    Dictionary<DistanceCalculation, List<IMovementMode>> template)
    {
        this.state = PathfinderRegionEdgeDataState.Modified;
        this.dataPool = dataPool;
        this.zoneDataByMovementStylePool = zoneDataByMovementStylePool;
        this.zoneDataByMovementModePool = zoneDataByMovementModePool;
        this.template = template;
        this.data = new Dictionary<TraversableZoneId, Dictionary<DistanceCalculation, Dictionary<IMovementMode, TraversableZonePathData>>>();
        this.zoneState = new Dictionary<TraversableZoneId, bool>();
    }

    public void Init(Position2D regionId)
    {
        this.regionId = regionId;
    }

    public void Clear()
    {
        foreach (var zone in data)
        {
            foreach (var style in zone.Value)
            {
                foreach (var mode in style.Value)
                {
                    mode.Value.Clear();
                    dataPool.Return(mode.Value);
                }

                style.Value.Clear();
                zoneDataByMovementModePool.Return(style.Value);
            }

            zone.Value.Clear();
            zoneDataByMovementStylePool.Return(zone.Value);
        }

        data.Clear();
        state = PathfinderRegionEdgeDataState.Modified;
    }

    public PathfinderRegionEdgeDataState State => state;

    public BufferList<GlobalTraversableZoneId> GetZones(BufferList<GlobalTraversableZoneId>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        foreach (var k in data.Keys)
        {
            var id = new GlobalTraversableZoneId(regionId, k);
            buffer.Add(id);
        }

        return buffer;
    }
    
    public BufferList<TraversableZonePathData> GetZoneData(GlobalTraversableZoneId zone, BufferList<TraversableZonePathData>? buffer = null)
    {
        var result = BufferList.PrepareBuffer(buffer);
        if (zone.RegionId == regionId && data.TryGetValue(zone.ZoneId, out var zoneData))
        {
            foreach (var zoneEntry in zoneData.Values)
            {
                foreach (var entry in zoneEntry.Values)
                {
                    result.Add(entry);
                }
            }
        }

        return result;
    }

    public bool TryGetZone(GlobalTraversableZoneId zone,
                           DistanceCalculation calc,
                           IMovementMode mode,
                           [MaybeNullWhen(false)] out TraversableZonePathData result,
                           DataViewCreateMode creationMode = DataViewCreateMode.Nothing)
    {
        if (zone.RegionId != regionId)
        {
            result = default;
            return false;
        }

        if (data.TryGetValue(zone.ZoneId, out var zoneData))
        {
            if (zoneData.TryGetValue(calc, out var zoneDataByStyle))
            {
                if (zoneDataByStyle.TryGetValue(mode, out result))
                {
                    return true;
                }

                if (creationMode == DataViewCreateMode.Nothing)
                {
                    return false;
                }
            }
            else
            {
                if (creationMode == DataViewCreateMode.Nothing)
                {
                    result = default;
                    return false;
                }
            }
        }
        else
        {
            if (creationMode == DataViewCreateMode.Nothing)
            {
                result = default;
                return false;
            }
        }

        result = Add(zone, calc, mode);
        return true;
    }

    TraversableZonePathData Add(GlobalTraversableZoneId zone, DistanceCalculation calc, IMovementMode mode)
    {
        var result = dataPool.Get();
        result.Init(new ZoneEdgeDataKey(zone, mode, calc));

        if (!data.TryGetValue(zone.ZoneId, out var zoneData))
        {
            zoneData = zoneDataByMovementStylePool.Get();
            data[zone.ZoneId] = zoneData;
        }

        if (!zoneData.TryGetValue(calc, out var zoneDataByMode))
        {
            zoneDataByMode = zoneDataByMovementModePool.Get();
            zoneData[calc] = zoneDataByMode;
        }

        zoneDataByMode[mode] = result;
        return result;
    }

    public void AddOutboundEdge(DistanceCalculation calc, in PathfinderRegionEdge edge)
    {
        if (edge.TargetZone.RegionId != regionId)
        {
            throw new ArgumentException("Given edge is not part of this region");
        }
        
        if (!template.TryGetValue(calc, out var knownMovementModes))
        {
            return;
        }

        foreach (var mode in knownMovementModes)
        {
            if (TryGetZone(edge.OwnerId, calc, mode, out var zoneData, DataViewCreateMode.CreateMissing))
            {
                zoneData.AddOutboundEdge(edge);
                zoneState[edge.OwnerId.ZoneId] = true;
                state = PathfinderRegionEdgeDataState.Modified;
            }
        }
    }

    public void AddInboundEdge(DistanceCalculation style, IMovementMode mode, 
                               PathfinderRegionEdge edge)
    {
        if (edge.TargetZone.RegionId != regionId)
        {
            throw new ArgumentException("Given edge is not part of this region");
        }
        
        if (TryGetZone(edge.TargetZone, style, mode, out var zone, DataViewCreateMode.CreateMissing))
        {
            zone.AddInboundEdge(edge);
            state = PathfinderRegionEdgeDataState.Modified;
            zoneState[edge.TargetZone.ZoneId] = true;
        }
    }

    public bool IsDirty(GlobalTraversableZoneId zoneId)
    {
        if (zoneId.RegionId != regionId)
        {
            throw new ArgumentException("Given edge is not part of this region");
        }
        
        if (zoneState.TryGetValue(zoneId.ZoneId, out var zoneDirtyFlag))
        {
            return zoneDirtyFlag;
        }

        return false;
    }

    public void MarkClean()
    {
        state = PathfinderRegionEdgeDataState.Clean;
        zoneState.Clear();
    }
}