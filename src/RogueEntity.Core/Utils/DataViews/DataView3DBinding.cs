using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils.DataViews;

static class DataView3DBinding
{
    static readonly ObjectPool<BufferList<Rectangle>> bufferPool;
    static readonly ObjectProvider provider;
    
    class ObjectProvider : IPooledObjectProvider<BufferList<Rectangle>>
    {
        public void Return(BufferList<Rectangle> t)
        {
            bufferPool.Return(t);
        }
    }

    static DataView3DBinding()
    {
        provider = new ObjectProvider();
        bufferPool = new DefaultObjectPool<BufferList<Rectangle>>(new BufferListObjectPoolPolicy<Rectangle>());
    }

    public static PooledObjectHandle<BufferList<Rectangle>> GetBuffer()
    {
        return new PooledObjectHandle<BufferList<Rectangle>>(provider, bufferPool.Get());
    }

    public static void ReturnBuffer(BufferList<Rectangle> obj)
    {
        bufferPool.Return(obj);
    }
}

public class DataView3DBinding<TSource, TTarget>: IDisposable
{
    class LayerBinding
    {
        readonly DataView3DBinding<TSource, TTarget> owner;
        int z;
        IReadOnlyDynamicDataView2D<TSource>? source;

        public LayerBinding(DataView3DBinding<TSource, TTarget> owner)
        {
            this.owner = owner;
        }

        public void Activate(int z, IReadOnlyDynamicDataView2D<TSource>? source)
        {
            if (source != null)
            {
                this.z = z;
                this.source = source;
                this.source.ViewChunkCreated += OnViewChunkCreated;
                this.source.ViewChunkExpired += OnViewChunkExpired;
                
                using var b = DataView3DBinding.GetBuffer();
                var tiles = source.GetActiveTiles(b);
                foreach (var tile in tiles)
                {
                    if (source.TryGetData(tile.X, tile.Y, out var raw))
                    {
                        owner.OnChunkCreated(z, source, raw);
                    }
                }
            }
        }

        public void Deactivate()
        {
            if (source == null) return;
            source.ViewChunkCreated -= OnViewChunkCreated;
            source.ViewChunkExpired -= OnViewChunkExpired;
        }
        
        void OnViewChunkExpired(object sender, DynamicDataView2DEventArgs<TSource> e)
        {
            if (source == null) return;
            owner.OnChunkExpired(z, source, e.Data);
        }

        void OnViewChunkCreated(object sender, DynamicDataView2DEventArgs<TSource> e)
        {
            if (source == null) return;
            owner.OnChunkCreated(z, source, e.Data);
        }
    }

    readonly IReadOnlyDynamicDataView3D<TSource> sourceView;
    readonly IDynamicDataView3D<TTarget> targetView;
    readonly Dictionary<int, LayerBinding> layerBindings;
    readonly IAggregateDynamicDataView3D<TSource>? aggregateSource;

    public DataView3DBinding(IReadOnlyDynamicDataView3D<TSource> sourceView, 
                             IDynamicDataView3D<TTarget> targetView)
    {
        this.sourceView = sourceView;
        this.targetView = targetView;
        this.layerBindings = new Dictionary<int, LayerBinding>();
        this.sourceView.ViewCreated += OnViewCreated;
        this.sourceView.ViewExpired += OnViewExpired;

        if (sourceView is IAggregateDynamicDataView3D<TSource> aggregationSource)
        {
            aggregateSource = aggregationSource;
            aggregateSource.ViewProcessed += OnViewProcessed;
        }
        else
        {
            aggregateSource = null;
        }

        foreach (var l in sourceView.GetActiveLayers())
        {
            if (sourceView.TryGetView(l, out var view))
            {
                ActivateLayerBinding(l, view);
            }
        }
    }

    void OnViewProcessed(object sender, AggregationViewProcessedEvent<TSource> e)
    {
        if (!targetView.TryGetWritableView(e.ZInfo, out var view2D, DataViewCreateMode.CreateMissing))
        {
            return;
        }

        if (view2D.TryGetWriteAccess(e.Tile.Bounds.X, e.Tile.Bounds.Y, out var tile, DataViewCreateMode.CreateMissing))
        {
            OnSourceViewProcessed(e.ZInfo, tile);
        }
    }

    protected virtual void OnSourceViewProcessed(int zInfo, IBoundedDataView<TTarget> tile)
    {
    }

    void OnChunkCreated(int z, IReadOnlyDynamicDataView2D<TSource> source, IReadOnlyBoundedDataView<TSource> tileData)
    {
        if (!targetView.TryGetWritableView(z, out var view2D, DataViewCreateMode.CreateMissing))
        {
            return;
        }

        view2D.TryGetWriteAccess(tileData.Bounds.X, tileData.Bounds.Y, out _, DataViewCreateMode.CreateMissing);
    }

    void OnChunkExpired(int z, IReadOnlyDynamicDataView2D<TSource> source, IReadOnlyBoundedDataView<TSource> tileData)
    {
        if (!targetView.TryGetWritableView(z, out var view2D))
        {
            return;
        }

        view2D.RemoveView(tileData.Bounds.X, tileData.Bounds.Y, out _);
    }

    public void Dispose()
    {
        foreach (var b in layerBindings)
        {
            b.Value.Deactivate();
        }

        if (aggregateSource != null)
        {
            aggregateSource.ViewProcessed -= OnViewProcessed;
        }
        
        this.sourceView.ViewCreated -= OnViewCreated;
        this.sourceView.ViewExpired -= OnViewExpired;
        this.layerBindings.Clear();
    }

    void ActivateLayerBinding(int z, IReadOnlyDynamicDataView2D<TSource> view)
    {
        if (!layerBindings.TryGetValue(z, out var binding))
        {
            binding = new LayerBinding(this);
        }
        
        binding.Activate(z, view);
        layerBindings[z] = binding;
    }

    void DeactivateLayerBinding(int z)
    {
        if (!layerBindings.TryGetValue(z, out var binding))
        {
            return;
        }

        binding.Deactivate();
        
    }

    void OnViewCreated(object sender, DynamicDataView3DEventArgs<TSource> e)
    {
        ActivateLayerBinding(e.ZLevel, e.Data);
    }

    void OnViewExpired(object sender, DynamicDataView3DEventArgs<TSource> e)
    {
        DeactivateLayerBinding(e.ZLevel);
    }
}