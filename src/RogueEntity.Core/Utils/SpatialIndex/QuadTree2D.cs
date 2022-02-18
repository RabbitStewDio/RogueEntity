using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils.DataViews;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public class QuadTree2D<T>: IQuadTreeAdapter, ISpatialIndex2D<T>
    {
        readonly DynamicDataViewConfiguration config;
        readonly int maxElementsPerNode;
        readonly int maxDepth;
        readonly List<BoundingBox> partitions;
        readonly FreeList<QuadElement> elements;
        readonly Dictionary<TileIndex, QuadTreeCore<QuadTree2D<T>>> spatialIndex;

        public QuadTree2D(DynamicDataViewConfiguration config, int maxElementsPerNode = 1, int maxDepth = 1)
        {
            this.config = config;
            this.maxElementsPerNode = maxElementsPerNode;
            this.maxDepth = maxDepth;
            this.partitions = new List<BoundingBox>();
            
            elements = new FreeList<QuadElement>();
            spatialIndex = new Dictionary<TileIndex, QuadTreeCore<QuadTree2D<T>>>();
            //spatialIndex = new QuadTreeCore<QuadTree<T>>(this, bounds, maxElementsPerNode, maxDepth);
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
                    localIndex = new QuadTreeCore<QuadTree2D<T>>(this, p, maxElementsPerNode, maxDepth);
                    spatialIndex[partitionKey] = localIndex;
                }

                localIndex.InsertElement(elementHandle, bounds);
            }

            return elementHandle;
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

        public List<FreeListIndex> Query(in BoundingBox bb, List<FreeListIndex> result, FreeListIndex skipElement = default)
        {
            
            var resultDeduplicator = ArrayPool<bool>.Shared.Rent(ElementIndexRange);
            try
            {
                var queryCollector = result ?? new List<FreeListIndex>();
                
                bb.PartitionBy(config, partitions);
                for (var index = 0; index < partitions.Count; index++)
                {
                    var p = partitions[index];
                    var partitionKey = new TileIndex(p.Left, p.Top);
                    if (spatialIndex.TryGetValue(partitionKey, out var localIndex))
                    {
                        
                        localIndex.Query(bb, queryCollector, resultDeduplicator, skipElement);
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

        public bool TryGet(FreeListIndex index, out T data, out BoundingBox boundingBox)
        {
            if (elements.TryGetValue(index, out var node))
            {
                data = node.Data;
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

        public bool TryGetDebugData(FreeListIndex index, out string data)
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
        internal readonly struct QuadElement: ISmartFreeListElement<QuadElement>
        {
            public FreeListIndex FreePointer => FreeListIndex.Of(Bounds.Top);

            public QuadElement AsFreePointer(FreeListIndex ptr)
            {
                return new QuadElement(default, new BoundingBox(ptr.Value, -1, -1, -1));
            }

            // Stores the ID for the element (can be used to
            // refer to external data).
            public readonly T Data;

            // Stores the rectangle for the element.
            public readonly BoundingBox Bounds;

            public QuadElement(T data, in BoundingBox bounds)
            {
                Data = data;
                Bounds = bounds;
            }
        }

    }
}
