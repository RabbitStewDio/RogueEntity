using EnTTSharp;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public class QuadTree2D<T> : ISpatialIndexAdapter, ISpatialIndex2D<T>
    {
        readonly ObjectPool<List<FreeListIndex>> freeListPool;
        readonly DynamicDataViewConfiguration config;
        readonly int maxElementsPerNode;
        readonly int maxDepth;
        readonly List<BoundingBox> partitions;
        readonly FreeList<QuadElement> elements;
        readonly Dictionary<TileIndex, QuadTreeCore<QuadTree2D<T>>> spatialIndex;

        public QuadTree2D(ObjectPool<List<FreeListIndex>> freeListPool,
                          DynamicDataViewConfiguration config,
                          int maxElementsPerNode = 1,
                          int maxDepth = 4)
        {
            this.freeListPool = freeListPool;
            this.config = config;
            this.maxElementsPerNode = maxElementsPerNode;
            this.maxDepth = maxDepth;
            this.partitions = new List<BoundingBox>();

            elements = new FreeList<QuadElement>();
            spatialIndex = new Dictionary<TileIndex, QuadTreeCore<QuadTree2D<T>>>();
        }

        public FreeListIndex Insert(T data, in BoundingBox bounds)
        {
            var elementHandle = elements.Add(new QuadElement(data, bounds));
            bounds.PartitionBy(config, partitions);
            for (var index = 0; index < partitions.Count; index++)
            {
                var p = partitions[index];
                var partitionKey = new TileIndex(p.Left, p.Top);
                if (!spatialIndex.TryGetValue(partitionKey, out var localIndex))
                {
                    localIndex = new QuadTreeCore<QuadTree2D<T>>(this, p, maxElementsPerNode, maxDepth, freeListPool);
                    spatialIndex[partitionKey] = localIndex;
                }

                localIndex.InsertElement(elementHandle, bounds);
            }

            return elementHandle;
        }

        public void Clear()
        {
            foreach (var pair in spatialIndex)
            {
                pair.Value.Clear();
            }
            spatialIndex.Clear();
        }

        public void Remove(FreeListIndex elementIndex)
        {
            if (!elements.TryGetValue(elementIndex, out var element))
            {
                return;
            }

            var elementBounds = element.Bounds;
            elementBounds.PartitionBy(config, partitions);
            for (var index = 0; index < partitions.Count; index++)
            {
                var p = partitions[index];
                var partitionKey = new TileIndex(p.Left, p.Top);
                if (spatialIndex.TryGetValue(partitionKey, out var localIndex))
                {
                    localIndex.RemoveElement(elementIndex, elementBounds);
                }
            }

            elements.Remove(elementIndex);
        }

        public void RemoveBulk(Func<FreeListIndex, bool> elementSelector)
        {
            var count = 0;
            var removeList = ArrayPool<FreeListIndex>.Shared.Rent(elements.Range);
            for (int i = 0; i < elements.Range; i += 1)
            {
                var flx = FreeListIndex.Of(i);
                if (elements.TryGetValue(flx, out _) && elementSelector.Invoke(flx))
                {
                    removeList[count] = flx;
                    count += 1;
                }
            }
            
            foreach (var qte in spatialIndex)
            {
                qte.Value.RemoveIf(elementSelector, qte.Value.BoundingBox);
            }

            for (var index = 0; index < count; index++)
            {
                var flx = removeList[index];
                elements.Remove(flx);
            }
        }

        public BufferList<T> Query(in BoundingBox bb,
                                   BufferList<T>? result = default,
                                   FreeListIndex skipElement = default)
        {
            result = BufferList.PrepareBuffer(result);
            using var buffer = BufferListPool<FreeListIndex>.GetPooled();
            foreach (var idx in QueryIndex(bb, buffer))
            {
                if (this.elements.TryGetValue(idx, out var qe) &&
                    qe.Data.TryGetValue(out var data))
                {
                    result.Add(data);
                }
            }

            return result;
        }
        
        public BufferList<FreeListIndex> QueryIndex(in BoundingBox bb, 
                                               BufferList<FreeListIndex>? result = default, 
                                               FreeListIndex skipElement = default)
        {
            var resultDeduplicator = ArrayPool<bool>.Shared.Rent(ElementIndexRange);
            try
            {
                var queryCollector = BufferList.PrepareBuffer(result);

                bb.PartitionBy(config, partitions);
                for (var index = 0; index < partitions.Count; index++)
                {
                    var p = partitions[index];
                    var partitionKey = new TileIndex(p.Left, p.Top);
                    if (spatialIndex.TryGetValue(partitionKey, out var localIndex))
                    {
                        localIndex.Query(bb, resultDeduplicator, queryCollector, skipElement);
                    }
                }

                return queryCollector;
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(resultDeduplicator);
            }
        }

        public void CleanUp()
        {
            foreach (var v in spatialIndex.Values)
            {
                v.CleanUp();
            }
        }

        public string Print()
        {
            StringBuilder b = new StringBuilder();
            foreach (var (key, value) in spatialIndex)
            {
                b.AppendLine($"Partition: {key}");
                value.PrintInto(b);
                b.AppendLine();
            }

            return b.ToString();
        }

        public bool TryGet(FreeListIndex index, [MaybeNullWhen(false)] out T data, out BoundingBox boundingBox)
        {
            if (elements.TryGetValue(index, out var node) &&
                node.Data.TryGetValue(out data))
            {
                boundingBox = node.Bounds;
                return true;
            }

            data = default;
            boundingBox = default;
            return false;
        }

        public int ElementIndexRange => elements.Range;

        public bool TryGetBounds(FreeListIndex index, out BoundingBox boundingBox)
        {
            if (elements.TryGetValue(index, out var node))
            {
                boundingBox = node.Bounds;
                return true;
            }

            boundingBox = default;
            return false;
        }

        public bool TryGetDebugData(FreeListIndex index, [MaybeNullWhen(false)] out string data)
        {
            if (elements.TryGetValue(index, out var node))
            {
                data = $"{node.Data}";
                return true;
            }

            data = default;
            return false;
        }

        internal QuadElement this[FreeListIndex index] => elements[index];

        // Represents an element in the quadtree.
        internal readonly struct QuadElement : ISmartFreeListElement<QuadElement>
        {
            public FreeListIndex FreePointer => FreeListIndex.Of(Bounds.Left);

            public QuadElement AsFreePointer(FreeListIndex ptr)
            {
                return new QuadElement(default, BoundingBox.From(ptr.Value, -1, -1, -1));
            }

            // Stores the ID for the element (can be used to
            // refer to external data).
            public readonly Optional<T> Data;

            // Stores the rectangle for the element.
            public readonly BoundingBox Bounds;

            public QuadElement(Optional<T> data, in BoundingBox bounds)
            {
                Data = data;
                Bounds = bounds;
            }
        }
    }
}