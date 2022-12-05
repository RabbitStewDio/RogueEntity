using EnTTSharp;
using EnTTSharp.Entities;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Core.Utils.SpatialIndex;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.Continuous;

public class ContinuousMapDataContext<TItemId> : IMapDataContext<TItemId>
    where TItemId : struct, IEntityKey
{
    readonly ObjectPool<LayerData> layerPool;
    readonly ObjectPool<GridIndex2DCore> corePool;
    readonly MapLayer mapLayer;
    readonly Dictionary<int, LayerData> layers;
    readonly EqualityComparer<TItemId> equalityComparer;

    public ContinuousMapDataContext(MapLayer mapLayer,
                                           DynamicDataViewConfiguration config)
    {
        if (mapLayer == MapLayer.Indeterminate)
        {
            throw new ArgumentException();
        }

        this.layers = new Dictionary<int, LayerData>();
        this.corePool = new DefaultObjectPool<GridIndex2DCore>(new GridIndex2DCorePolicy(config));
        this.layerPool = new DefaultObjectPool<LayerData>(new LayerDataPolicy(corePool, config));
        this.equalityComparer = EqualityComparer<TItemId>.Default;
        this.mapLayer = mapLayer;
    }

    public bool AllowMultipleItems => true;

    public event EventHandler<PositionDirtyEventArgs>? PositionDirty;
    public event EventHandler<MapRegionDirtyEventArgs>? RegionDirty;

    public void ResetState()
    {
        if (layers.Count == 0)
        {
            return;
        }

        int zMin = int.MaxValue;
        int zMax = int.MinValue;
        foreach (var l in layers)
        {
            zMin = Math.Min(l.Key, zMin);
            zMax = Math.Max(l.Key, zMax);
            l.Value.Clear();
            layerPool.Return(l.Value);
        }

        layers.Clear();
        MarkRegionDirty(zMin, zMax);
    }

    public MapLayer Layer => mapLayer;

    public void ResetLevel(int z)
    {
        if (layers.TryGetValue(z, out var layer))
        {
            layer.Clear();
        }
    }

    public BufferList<int> GetActiveZLayers(BufferList<int>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        foreach (var k in layers)
        {
            buffer.Add(k.Key);
        }

        return buffer;
    }

    public BufferList<Rectangle> GetActiveTiles(int z, BufferList<Rectangle>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        if (layers.TryGetValue(z, out var ctx))
        {
            ctx.Data.GetActiveTiles(buffer);
        }

        return buffer;
    }

    public BufferList<(TItemId, TPosition)> QueryItemArea<TPosition>(in Rectangle area, int z, BufferList<(TItemId, TPosition)>? buffer = null) where TPosition : struct, IPosition<TPosition>
    {
        buffer = BufferList.PrepareBuffer(buffer);
        using var indexBuffer = BufferListPool<FreeListIndex>.GetPooled();
        if (!PositionTypeRegistry.Instance.TryGet<TPosition>(out var reg))
        {
            // should not happen due to auto-registration
            return buffer;
        }
        
        if (layers.TryGetValue(z, out var layer))
        {
            foreach (var idx in layer.Data.QueryIndex(area, indexBuffer))
            {
                if (layer.Data.TryGet(idx, out var itemId, out _) &&
                    layer.DetailedPositions.TryGetValue(idx, out var itemPosition))
                {
                    if (itemPosition.GridZ == z && area.Contains(itemPosition.ToGridXY()))
                    {
                        buffer.Add((itemId, reg.Convert(itemPosition)));
                    }
                }
            }
        }

        return buffer;
    }

    public BufferList<(TItemId, TPosition)> QueryItemTile<TPosition>(in EntityGridPosition position, BufferList<(TItemId, TPosition)>? buffer = null) where TPosition : struct, IPosition<TPosition>
    {
        var pos = position.ToGridXY();
        var z = position.GridZ;
        buffer = BufferList.PrepareBuffer(buffer);
        using var indexBuffer = BufferListPool<FreeListIndex>.GetPooled();
        if (!layers.TryGetValue(z, out var layer))
        {
            return buffer;
        }

        if (!PositionTypeRegistry.Instance.TryGet<TPosition>(out var reg))
        {
            // should not happen due to auto-registration
            return buffer;
        }
        
        foreach (var idx in layer.Data.QueryIndex(pos, indexBuffer))
        {
            if (layer.Data.TryGet(idx, out var itemId, out _) &&
                layer.DetailedPositions.TryGetValue(idx, out var itemPosition))
            {
                if (EntityGridPosition.From(itemPosition) == position)
                {
                    buffer.Add((itemId, reg.Convert(itemPosition)));
                }
            }
        }

        return buffer;
    }

    public BufferList<TItemId> QueryItem<TPosition>(in TPosition position, BufferList<TItemId>? buffer = null)
        where TPosition : struct, IPosition<TPosition>
    {
        var pos = position.ToGridXY();
        var z = position.GridZ;
        var cpos = ContinuousMapPosition.From(position);
        buffer = BufferList.PrepareBuffer(buffer);
        using var indexBuffer = BufferListPool<FreeListIndex>.GetPooled();
        if (!layers.TryGetValue(z, out var layer))
        {
            return buffer;
        }

        foreach (var idx in layer.Data.QueryIndex(pos, indexBuffer))
        {
            if (layer.Data.TryGet(idx, out var itemId, out _) &&
                layer.DetailedPositions.TryGetValue(idx, out var itemPosition))
            {
                if (cpos == itemPosition)
                {
                    buffer.Add(itemId);
                }
            }
        }

        return buffer;
    }


    public bool TryRemoveItem<TPosition>(TItemId itemId, in TPosition mapPosition)
        where TPosition : struct, IPosition<TPosition>
    {
        var pos = mapPosition.ToGridXY();
        var z = mapPosition.GridZ;
        if (!layers.TryGetValue(z, out var gridIndex))
        {
            return false;
        }

        using var buffer = BufferListPool<FreeListIndex>.GetPooled();
        foreach (var idx in gridIndex.Data.QueryIndex(pos, buffer))
        {
            if (gridIndex.Data.TryGet(idx, out var item, out _) &&
                equalityComparer.Equals(item, itemId))
            {
                gridIndex.Data.Remove(idx);
                return true;
            }
        }

        return false;
    }

    public bool TryInsertItem<TPosition>(TItemId itemId, in TPosition desiredPosition)
        where TPosition : struct, IPosition<TPosition>
    {
        var pos = desiredPosition.ToGridXY();
        var z = desiredPosition.GridZ;
        if (!layers.TryGetValue(z, out var gridIndex))
        {
            gridIndex = layerPool.Get();
            layers[z] = gridIndex;
        }

        var idx = gridIndex.Data.Insert(itemId, BoundingBox.From(pos));
        gridIndex.DetailedPositions[idx] = ContinuousMapPosition.From(desiredPosition);
        return true;
    }

    public bool TryUpdateItem<TPosition>(TItemId source, TItemId replacement, in TPosition desiredPosition) where TPosition : struct, IPosition<TPosition>
    {
        var pos = desiredPosition.ToGridXY();
        var z = desiredPosition.GridZ;
        if (!layers.TryGetValue(z, out var gridIndex))
        {
            return false;
        }

        var cpos = ContinuousMapPosition.From(desiredPosition);
        using var buffer = BufferListPool<FreeListIndex>.GetPooled();
        foreach (var idx in gridIndex.Data.QueryIndex(pos, buffer))
        {
            if (!gridIndex.Data.TryGet(idx, out var item, out _) ||
                !equalityComparer.Equals(item, source) ||
                !gridIndex.DetailedPositions.TryGetValue(idx, out var itemPosition) ||
                !itemPosition.Equals(cpos))
            {
                continue;
            }

            gridIndex.Data.TryUpdateIndex(idx, replacement);
            return true;
        }

        return false;
    }

    public void MarkDirty<TPosition>(in TPosition position)
        where TPosition : IPosition<TPosition>
    {
        PositionDirty?.Invoke(this, new PositionDirtyEventArgs(Position.From(position)));
    }

    public void MarkRegionDirty(int zPositionFrom, int zPositionTo, Optional<Rectangle> layerArea = default)
    {
        RegionDirty?.Invoke(this, new MapRegionDirtyEventArgs(zPositionFrom, zPositionTo, layerArea));
    }

    class LayerDataPolicy : IPooledObjectPolicy<LayerData>
    {
        readonly ObjectPool<GridIndex2DCore> corePool;
        readonly DynamicDataViewConfiguration config;

        public LayerDataPolicy(ObjectPool<GridIndex2DCore> corePool, DynamicDataViewConfiguration config)
        {
            this.corePool = corePool;
            this.config = config;
        }

        public LayerData Create()
        {
            return new LayerData(new GridIndex2D<TItemId>(corePool, config));
        }

        public bool Return(LayerData obj)
        {
            obj.Clear();
            return true;
        }
    }

    class LayerData
    {
        public readonly GridIndex2D<TItemId> Data;
        public readonly Dictionary<FreeListIndex, ContinuousMapPosition> DetailedPositions;

        public LayerData(GridIndex2D<TItemId> layerData)
        {
            this.Data = layerData;
            this.DetailedPositions = new Dictionary<FreeListIndex, ContinuousMapPosition>();
        }

        public void Clear()
        {
            Data.Clear();
            DetailedPositions.Clear();
        }
    }
}