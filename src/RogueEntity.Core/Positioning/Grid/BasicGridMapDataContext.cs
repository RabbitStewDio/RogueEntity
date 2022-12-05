using EnTTSharp;
using EnTTSharp.Entities;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Positioning.Grid;

public class BasicGridMapDataContext<TItemId> : IMapDataContext<TItemId>
    where TItemId : struct, IEntityKey
{
    readonly MapLayer layer;
    readonly DynamicDataViewConfiguration config;
    readonly PooledDynamicDataView3D<TItemId> dataView;
    readonly EqualityComparer<TItemId> equalityComparer;
    readonly ObjectPool<List<Rectangle>> partitionPool;

    public BasicGridMapDataContext(MapLayer layer, DynamicDataViewConfiguration config)
    {
        this.layer = layer;
        this.config = config;
        this.equalityComparer = EqualityComparer<TItemId>.Default;
        this.dataView = new PooledDynamicDataView3D<TItemId>(new DefaultBoundedDataViewPool<TItemId>(config));
        this.partitionPool = new DefaultObjectPool<List<Rectangle>>(new ListObjectPoolPolicy<Rectangle>());
    }

    public event EventHandler<PositionDirtyEventArgs>? PositionDirty;
    public event EventHandler<MapRegionDirtyEventArgs>? RegionDirty;

    public bool AllowMultipleItems => false;

    public void MarkDirty<TPosition>(in TPosition position)
        where TPosition : IPosition<TPosition>
    {
        PositionDirty?.Invoke(this, new PositionDirtyEventArgs(Position.From(position)));
    }

    public void MarkRegionDirty(int zPositionFrom, int zPositionTo, Optional<Rectangle> layerArea = default)
    {
        RegionDirty?.Invoke(this, new MapRegionDirtyEventArgs(zPositionFrom, zPositionTo, layerArea));
    }

    public void ResetLevel(int z)
    {
        dataView.RemoveView(z);
    }

    public MapLayer Layer => layer;

    public void ResetState()
    {
        using var layerBuffer = BufferListPool<int>.GetPooled();
        dataView.GetActiveLayers(layerBuffer);
        if (layerBuffer.Data.Count == 0)
        {
            return;
        }
        
        int zMin = int.MaxValue;
        int zMax = int.MinValue;
        foreach (var l in layerBuffer.Data)
        {
            zMin = Math.Min(l, zMin);
            zMax = Math.Max(l, zMax);
        }

        dataView.ExpireAll();
        MarkRegionDirty(zMin, zMax);
    }

    public BufferList<Rectangle> GetActiveTiles(int z, BufferList<Rectangle>? buffer = null)
    {
        buffer = BufferList.PrepareBuffer(buffer);
        if (dataView.TryGetView(z, out var ctx))
        {
            ctx.GetActiveTiles(buffer);
        }

        return buffer;
    }

    public BufferList<int> GetActiveZLayers(BufferList<int>? buffer = null)
    {
        return dataView.GetActiveLayers(buffer);
    }

    public BufferList<(TItemId, TPosition)> QueryItemArea<TPosition>(in Rectangle area, int z, BufferList<(TItemId, TPosition)>? buffer = null) where TPosition : struct, IPosition<TPosition>
    {
        buffer = BufferList.PrepareBuffer(buffer);
        if (!dataView.TryGetView(z, out var data))
        {
            return buffer;
        }

        if (!PositionTypeRegistry.Instance.TryGet<TPosition>(out var reg))
        {
            // should not happen due to auto-registration
            return buffer;
        }

        var origin = EntityGridPosition.OfRaw(layer.LayerId, area.X, area.Y, z);
        var partitions = partitionPool.Get();
        try
        {
            foreach (var partition in area.PartitionBy(config, partitions))
            {
                if (!data.TryGetData(partition.X, partition.Y, out var region))
                {
                    continue;
                }

                foreach (var pos in partition.GetIntersection(area).Contents)
                {
                    if (region.TryGet(pos.X, pos.Y, out var item) &&
                        !equalityComparer.Equals(item, default))
                    {
                        buffer.Add((item, reg.Convert(origin.WithPosition(pos.X, pos.Y))));
                    }
                }
            }
        }
        finally
        {
            partitionPool.Return(partitions);
        }
        return buffer;
    }


    public BufferList<(TItemId, TPosition)> QueryItemTile<TPosition>(in EntityGridPosition position, BufferList<(TItemId, TPosition)>? buffer = null) where TPosition : struct, IPosition<TPosition>
    {
        buffer = BufferList.PrepareBuffer(buffer);
        if (!PositionTypeRegistry.Instance.TryGet<TPosition>(out var reg))
        {
            // should not happen due to auto-registration
            return buffer;
        }
        
        var z = position.GridZ;
        var xy = position.ToGridXY();
        if (dataView.TryGetView(z, out var data) && data.TryGet(xy.X, xy.Y, out var item))
        {
            if (!equalityComparer.Equals(item, default))
            {
                buffer.Add((item, reg.Convert(EntityGridPosition.From(position))));
            }
        }

        return buffer;
    }

    public BufferList<TItemId> QueryItem<TPosition>(in TPosition position, BufferList<TItemId>? buffer = null) where TPosition : struct, IPosition<TPosition>
    {
        buffer = BufferList.PrepareBuffer(buffer);
        var z = position.GridZ;
        var xy = position.ToGridXY();
        if (dataView.TryGetView(z, out var data) &&
            data.TryGet(xy.X, xy.Y, out var item))
        {
            if (!equalityComparer.Equals(item, default))
            {
                buffer.Add(item);
            }
        }

        return buffer;
    }

    public bool TryRemoveItem<TPosition>(TItemId itemId, in TPosition pos)
        where TPosition : struct, IPosition<TPosition>
    {
        var z = pos.GridZ;
        var xy = pos.ToGridXY();
        if (dataView.TryGetWritableView(z, out var data))
        {
            return data.TrySet(xy.X, xy.Y, default);
        }

        return false;
    }

    public bool TryInsertItem<TPosition>(TItemId itemId, in TPosition desiredPosition) where TPosition : struct, IPosition<TPosition>
    {
        var z = desiredPosition.GridZ;
        var xy = desiredPosition.ToGridXY();
        if (dataView.TryGetWritableView(z, out var data, DataViewCreateMode.CreateMissing))
        {
            return data.TrySet(xy.X, xy.Y, itemId);
        }

        return false;
    }

    public bool TryUpdateItem<TPosition>(TItemId source, TItemId replacement, in TPosition desiredPosition) where TPosition : struct, IPosition<TPosition>
    {
        if (equalityComparer.Equals(source, replacement)) return true;
        return TryInsertItem(replacement, desiredPosition);
    }
}