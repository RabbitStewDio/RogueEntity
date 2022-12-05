using EnTTSharp;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public class GridIndex2D<T> : ISpatialIndex2D<T>, ISpatialIndexAdapter
    {
        readonly ObjectPool<GridIndex2DCore> pool;
        readonly FreeList<GridElement> elements;
        readonly DynamicDataViewConfiguration config;
        readonly Dictionary<TileIndex, GridIndex2DCore> regions;

        public GridIndex2D(ObjectPool<GridIndex2DCore> pool, DynamicDataViewConfiguration config)
        {
            this.pool = pool;
            this.config = config;
            this.regions = new Dictionary<TileIndex, GridIndex2DCore>();
            this.elements = new FreeList<GridElement>();
        }

        public GridIndex2D(DynamicDataViewConfiguration config) : 
            this(new DefaultObjectPool<GridIndex2DCore>(new GridIndex2DCorePolicy(config)), config)
        {
        }

        public void Clear()
        {
            foreach (var c in regions)
            {
                c.Value.Clear();
            }            
        }
        
        public int ElementIndexRange => elements.Range;

        public bool TryGetBounds(FreeListIndex index, out BoundingBox boundingBox)
        {
            if (elements.TryGetValue(index, out var ge))
            {
                boundingBox = ge.Bounds;
                return true;
            }

            boundingBox = default;
            return false;
        }

        public bool TryGetDebugData(FreeListIndex index, [MaybeNullWhen(false)] out string data)
        {
            if (elements.TryGetValue(index, out var ge))
            {
                data = $"{ge.Data}";
                return true;
            }

            data = "";
            return false;
        }

        public FreeListIndex Insert(T data, in BoundingBox bounds)
        {
            var elementIndex = elements.Add(new GridElement(data, bounds));

            var (left, top) = config.TileIndex(bounds.Left, bounds.Top);
            var (right, bottom) = config.TileIndex(bounds.Right, bounds.Bottom);
            for (int y = top; y <= bottom; y += 1)
            {
                for (int x = left; x <= right; x += 1)
                {
                    var tx = new TileIndex(x, y);
                    if (!regions.TryGetValue(tx, out var region))
                    {
                        var regionBounds = config.Bounds(tx);
                        region = pool.Get();
                        region.Reuse(this, regionBounds);
                        regions[tx] = region;
                    }

                    region.Insert(elementIndex, bounds);
                }
            }

            return elementIndex;
        }

        internal GridElement this[FreeListIndex index] => elements[index];

        public bool TryGet(FreeListIndex index, [MaybeNullWhen(false)] out T data, out BoundingBox boundingBox)
        {
            if (elements.TryGetValue(index, out var ge) &&
                ge.Data.TryGetValue(out data))
            {
                boundingBox = ge.Bounds;
                return true;
            }

            data = default;
            boundingBox = default;
            return false;
        }

        public bool TryUpdateIndex(FreeListIndex index, T data)
        {
            if (elements.TryGetValue(index, out var ge))
            {
                ge = new GridElement(data, ge.Bounds);
                elements.Replace(index, ge);
                return true;
            }

            return false;
        }
        
        public void Remove(FreeListIndex elementIndex)
        {
            if (!elements.TryGetValue(elementIndex, out var ge))
            {
                return;
            }

            var bounds = ge.Bounds;

            var (left, top) = config.TileIndex(bounds.Left, bounds.Top);
            var (right, bottom) = config.TileIndex(bounds.Right, bounds.Bottom);
            for (int y = top; y <= bottom; y += 1)
            {
                for (int x = left; x <= right; x += 1)
                {
                    var tx = new TileIndex(x, y);
                    if (regions.TryGetValue(tx, out var region))
                    {
                        region.Remove(elementIndex, bounds);
                    }
                }
            }

            elements.Remove(elementIndex);
        }

        public BufferList<FreeListIndex> QueryIndex(in Position2D pos,
                                                    BufferList<FreeListIndex>? result = null,
                                                    FreeListIndex skipElement = default)
        {
            result = BufferList.PrepareBuffer(result);

            var resultDeduplicator = ArrayPool<bool>.Shared.Rent(elements.Range);
            try
            {
                Array.Clear(resultDeduplicator, 0, elements.Range);
                var bb = BoundingBox.From(pos);
                var (x, y) = config.TileIndex(pos.X, pos.Y);
                var tx = new TileIndex(x, y);
                if (regions.TryGetValue(tx, out var region))
                {
                    region.QueryIndex(bb, resultDeduplicator, result, skipElement);
                }

                return result;
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(resultDeduplicator);
            }
            
        }
        
        public BufferList<FreeListIndex> QueryIndex(in BoundingBox bb, 
                                                    BufferList<FreeListIndex>? result = null, 
                                                    FreeListIndex skipElement = default)
        {
            result = BufferList.PrepareBuffer(result);

            var resultDeduplicator = ArrayPool<bool>.Shared.Rent(elements.Range);
            try
            {
                Array.Clear(resultDeduplicator, 0, elements.Range);

                var (left, top) = config.TileIndex(bb.Left, bb.Top);
                var (right, bottom) = config.TileIndex(bb.Right, bb.Bottom);
                for (int y = top; y <= bottom; y += 1)
                {
                    for (int x = left; x <= right; x += 1)
                    {
                        var tx = new TileIndex(x, y);
                        if (regions.TryGetValue(tx, out var region))
                        {
                            region.QueryIndex(bb, resultDeduplicator, result, skipElement);
                        }
                    }
                }

                return result;
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(resultDeduplicator);
            }
        }

        public BufferList<T> Query(in Position2D pos,
                                   BufferList<T>? result = default,
                                   FreeListIndex skipElement = default)
        {
            result = BufferList.PrepareBuffer(result);
            using var buffer = BufferListPool<FreeListIndex>.GetPooled();
            foreach (var idx in QueryIndex(pos, buffer, skipElement))
            {
                if (this.elements.TryGetValue(idx, out var qe) &&
                    qe.Data.TryGetValue(out var data))
                {
                    result.Add(data);
                }
            }

            return result;
        }

        public BufferList<T> Query(in BoundingBox bb,
                                   BufferList<T>? result = default,
                                   FreeListIndex skipElement = default)
        {
            result = BufferList.PrepareBuffer(result);
            using var buffer = BufferListPool<FreeListIndex>.GetPooled();
            foreach (var idx in QueryIndex(bb, buffer, skipElement))
            {
                if (this.elements.TryGetValue(idx, out var qe) &&
                    qe.Data.TryGetValue(out var data))
                {
                    result.Add(data);
                }
            }

            return result;
        }

        // Represents an element in the quadtree.
        public readonly struct GridElement : ISmartFreeListElement<GridElement>
        {
            public FreeListIndex FreePointer => FreeListIndex.Of(Bounds.Left);

            public GridElement AsFreePointer(FreeListIndex ptr)
            {
                return new GridElement(default, BoundingBox.From(ptr.Value, -1, -1, -1));
            }

            // Stores the ID for the element (can be used to
            // refer to external data).
            public readonly Optional<T> Data;

            // Stores the rectangle for the element.
            public readonly BoundingBox Bounds;

            public GridElement(Optional<T> data, in BoundingBox bounds)
            {
                Data = data;
                Bounds = bounds;
            }
        }

        public BufferList<Rectangle> GetActiveTiles(BufferList<Rectangle>? buffer = null)
        {
            buffer = BufferList.PrepareBuffer(buffer);

            foreach (var x in this.regions)
            {
                if (x.Value.ContainsAnyContent)
                {
                    buffer.Add(x.Value.RegionBounds);
                }
            }
            
            return buffer;
        }
    }
}