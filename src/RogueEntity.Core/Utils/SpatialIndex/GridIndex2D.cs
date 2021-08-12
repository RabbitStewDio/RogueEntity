using RogueEntity.Core.Utils.DataViews;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public class GridIndex2D<T> : ISpatialIndex2D<T>
    {
        readonly DynamicDataViewConfiguration cellConfig;
        readonly FreeList<GridElement> elements;
        readonly FreeList<GridElementNode> elementNodes;
        readonly Dictionary<TileIndex, int> elementGrid;

        public GridIndex2D(DynamicDataViewConfiguration config)
        {
            this.cellConfig = config;

            elements = new FreeList<GridElement>();
            elementNodes = new FreeList<GridElementNode>();
            elementGrid = new Dictionary<TileIndex, int>();
        }

        public int Insert(T data, in BoundingBox bounds)
        {
            var e = elements.Add(new GridElement(data, bounds));

            var ul = cellConfig.TileIndex(bounds.Left, bounds.Top);
            var lr = cellConfig.TileIndex(bounds.Right, bounds.Bottom);
            for (int ty = ul.Y; ty <= lr.Y; ty += 1)
            {
                for (int tx = ul.X; tx <= lr.X; tx += 1)
                {
                    InsertCellElement(tx, ty, e);
                }
            }

            return e;
        }

        void InsertCellElement(int tx, int ty, int elementIndex)
        {
            var tileIndex = new TileIndex(tx, ty);
            if (!elementGrid.TryGetValue(tileIndex, out var node))
            {
                node = -1;
            }
            
            var newNode = new GridElementNode(node, elementIndex);
            var newNodeIdx = elementNodes.Add(newNode);
            elementGrid[tileIndex] = newNodeIdx;
        }

        public bool TryGet(int index, out T data, out BoundingBox boundingBox)
        {
            if (elements.TryGetValue(index, out var ge))
            {
                data = ge.Data;
                boundingBox = ge.Bounds;
                return true;
            }

            data = default;
            boundingBox = default;
            return false;
        }

        internal GridElement this[int index] => elements[index]; 
        
        public void Remove(int elementIndex)
        {
            if (!elements.TryGetValue(elementIndex, out var ge))
            {
                return;
            }

            var bounds = ge.Bounds;
            var ul = cellConfig.TileIndex(bounds.Left, bounds.Top);
            var lr = cellConfig.TileIndex(bounds.Right, bounds.Bottom);
            for (int ty = ul.Y; ty <= lr.Y; ty += 1)
            {
                for (int tx = ul.X; tx <= lr.X; tx += 1)
                {
                    RemoveCellElement(tx, ty, elementIndex);
                }
            }
            
            elements.Remove(elementIndex);
        }

        void RemoveCellElement(int tx, int ty, int elementToRemove)
        {
            if (!elementGrid.TryGetValue(new TileIndex(tx, ty), out var elementNodeIndex))
            {
                return;
            }
            
            var prevElementNodeIndex = -1;
            while (elementNodeIndex != -1 &&
                   elementNodes[elementNodeIndex].ElementIndex != elementToRemove)
            {
                prevElementNodeIndex = elementNodeIndex;
                elementNodeIndex = elementNodes[elementNodeIndex].NextElementNodeIndex;
            }

            if (elementNodeIndex != -1)
            {
                var nextIndex = elementNodes[elementNodeIndex].NextElementNodeIndex;
                if (prevElementNodeIndex == -1)
                {
                    // first element in the linked list
                    elementGrid[new TileIndex(tx, ty)] = nextIndex;
                    elementNodes.Remove(nextIndex);
                }
                else
                {
                    // middle or end, so we restitch the linked list around the removed element
                    var prevNode = elementNodes[prevElementNodeIndex];
                    elementNodes.Replace(prevElementNodeIndex, new GridElementNode(nextIndex, prevNode.ElementIndex));
                    elementNodes.Remove(nextIndex);
                }
            }

        }

        public List<int> Query(in BoundingBox bb, List<int> result, int skipElement = -1)
        {
            if (result == null)
            {
                result = new List<int>();
            }
            else
            {
                result.Clear();
            }

            var resultDeduplicator = ArrayPool<bool>.Shared.Rent(elements.Range);
            try
            {
                var ul = cellConfig.TileIndex(bb.Left, bb.Top);
                var lr = cellConfig.TileIndex(bb.Right, bb.Bottom);
                Array.Clear(resultDeduplicator, 0, elements.Range);

                for (int ty = ul.Y; ty <= lr.Y; ty += 1)
                {
                    for (int tx = ul.X; tx <= lr.X; tx += 1)
                    {
                        QueryCellElement(new TileIndex(tx, ty), bb, result, resultDeduplicator, skipElement);
                    }
                }

                return result;
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(resultDeduplicator);
            }
        }

        public string Print()
        {
            StringBuilder b = new StringBuilder();
            foreach (var t in elementGrid)
            {
                b.AppendLine($"Grid Cell: {t.Key}");
                var firstElementIdx = t.Value;
                while (firstElementIdx != -1)
                {
                    var elementNode = elementNodes[firstElementIdx];
                    var elementIndex = elementNode.ElementIndex;
                    var element = elements[elementIndex];

                    b.AppendLine($"  ${elementIndex}: {element.Bounds} -> {element.Data}");
                    
                    firstElementIdx = elementNode.NextElementNodeIndex;
                }
            }

            return b.ToString();
        }

        void QueryCellElement(in TileIndex tx, in BoundingBox bb, List<int> result, bool[] resultDeduplicator, int skipElement)
        {
            if (!elementGrid.TryGetValue(tx, out var firstElementIdx))
            {
                return;
            }

            while (firstElementIdx != -1)
            {
                var elementNode = elementNodes[firstElementIdx];
                var elementIndex = elementNode.ElementIndex;
                var element = elements[elementIndex];
                if (!resultDeduplicator[elementIndex] &&
                    elementIndex != skipElement)
                {
                    if (element.Bounds.Intersects(bb))
                    {
                        result.Add(elementIndex);
                        resultDeduplicator[elementIndex] = true;
                    }
                }

                firstElementIdx = elementNode.NextElementNodeIndex;
            }
        }

        // Represents an element in the quadtree.
        public readonly struct GridElement : ISmartFreeListElement<GridElement>
        {
            public int FreePointer => Bounds.Top;

            public GridElement AsFreePointer(int ptr)
            {
                return new GridElement(default, new BoundingBox(ptr, -1, -1, -1));
            }

            // Stores the ID for the element (can be used to
            // refer to external data).
            public readonly T Data;

            // Stores the rectangle for the element.
            public readonly BoundingBox Bounds;

            public GridElement(T data, in BoundingBox bounds)
            {
                Data = data;
                Bounds = bounds;
            }
        }

        // Represents an element node in the quadtree.
        readonly struct GridElementNode : ISmartFreeListElement<GridElementNode>
        {
            public int FreePointer => NextElementNodeIndex;

            public GridElementNode AsFreePointer(int ptr)
            {
                return new GridElementNode(ptr, -1);
            }

            // Points to the next element in the leaf node. A value of -1 
            // indicates the end of the list. This points to elements in QuadTree::elementNodes.
            public readonly int NextElementNodeIndex;

            // Stores the index to the actual data element (held in QuadTree::elements).
            public readonly int ElementIndex;

            public GridElementNode(int nextElementNodeIndex, int elementIndex)
            {
                NextElementNodeIndex = nextElementNodeIndex;
                ElementIndex = elementIndex;
            }
        }
    }
}
