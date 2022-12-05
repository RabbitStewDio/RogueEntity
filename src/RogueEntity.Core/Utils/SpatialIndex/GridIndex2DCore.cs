using EnTTSharp;
using RogueEntity.Api.Utils;
using System;
using System.Text;

namespace RogueEntity.Core.Utils.SpatialIndex;

public class GridIndex2DCore
{
    Optional<ISpatialIndexAdapter> parent;
    Rectangle regionBounds;
    readonly FreeList<GridElementNode> elementNodes;
    readonly FreeListIndex[] elementGrid;

    public GridIndex2DCore(ISpatialIndexAdapter parent, Rectangle regionBounds)
    {
        this.parent = Optional.OfNullable(parent);
        this.regionBounds = regionBounds;
        this.elementNodes = new FreeList<GridElementNode>();
        this.elementGrid = new FreeListIndex[regionBounds.Width * regionBounds.Height];
    }

    public GridIndex2DCore(Rectangle regionBounds)
    {
        this.parent = default;
        this.regionBounds = regionBounds;
        this.elementNodes = new FreeList<GridElementNode>();
        this.elementGrid = new FreeListIndex[regionBounds.Width * regionBounds.Height];
    }

    public Rectangle RegionBounds => regionBounds;

    public bool ContainsAnyContent => !elementNodes.IsEmpty;
    
    public void Reuse(ISpatialIndexAdapter parent, Rectangle regionBounds)
    {
        this.parent = Optional.OfNullable(parent);
        if (regionBounds.Width != this.regionBounds.Width &&
            regionBounds.Height != this.regionBounds.Height)
        {
            throw new ArgumentException();
        }
        
        this.regionBounds = regionBounds;
        
    }

    public void Return()
    {
        this.parent = default;
        this.regionBounds = default;
    }
    
    public void Clear()
    {
        elementNodes.Clear();
        Array.Fill(elementGrid, FreeListIndex.Empty);
    }
    
    public FreeListIndex Insert(FreeListIndex e, in BoundingBox bounds)
    {
        var intersection = this.regionBounds.GetIntersection(bounds);
        foreach (var (tx, ty) in intersection.Contents)
        {
            InsertCellElement(tx, ty, e);
        }

        return e;
    }

    void InsertCellElement(int tx, int ty, FreeListIndex elementIndex)
    {
        var x = tx - regionBounds.X;
        var y = ty - regionBounds.Y;
        ref var node = ref elementGrid[x + y * regionBounds.Width]; 
        var newNode = new GridElementNode(node, elementIndex);
        var newNodeIdx = elementNodes.Add(newNode);
        node = newNodeIdx;
    }
        
    public void Remove(FreeListIndex elementIndex, BoundingBox bounds)
    {
        var intersection = this.regionBounds.GetIntersection(bounds);
        foreach (var (tx, ty) in intersection.Contents)
        {
            RemoveCellElement(tx, ty, elementIndex);
        }
    }

    void RemoveCellElement(int tx, int ty, FreeListIndex elementToRemove)
    {
        var x = tx - regionBounds.X;
        var y = ty - regionBounds.Y;
        var elementNodeIndex = elementGrid[x + y * regionBounds.Width]; 

        if (elementNodeIndex == FreeListIndex.Empty)
        {
            return;
        }

        var prevElementNodeIndex = FreeListIndex.Empty;
        while (!elementNodeIndex.IsEmpty &&
               elementNodes[elementNodeIndex].ElementIndex != elementToRemove)
        {
            prevElementNodeIndex = elementNodeIndex;
            elementNodeIndex = elementNodes[elementNodeIndex].NextElementNodeIndex;
        }

        if (!elementNodeIndex.IsEmpty)
        {
            var nextIndex = elementNodes[elementNodeIndex].NextElementNodeIndex;
            if (prevElementNodeIndex.IsEmpty)
            {
                // first element in the linked list
                elementGrid[x + y * regionBounds.Width] = nextIndex;
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
        
    public BufferList<FreeListIndex> QueryIndex(in BoundingBox bb, 
                                                bool[] resultDeduplicator,
                                                BufferList<FreeListIndex>? result, 
                                                FreeListIndex skipElement = default)
    {
        result = BufferList.PrepareBuffer(result);
        var intersection = this.regionBounds.GetIntersection(bb);
        foreach (var (tx, ty) in intersection.Contents)
        {
            QueryCellElement(new Position2D(tx, ty), result, resultDeduplicator, skipElement);
        }

        return result;
    }

    public string Print()
    {
        if (!this.parent.TryGetValue(out var parentRef))
        {
            throw new InvalidOperationException();
        }
        
        StringBuilder b = new StringBuilder();
        foreach (var pos in regionBounds.Contents)
        {
            var t = elementGrid[pos.X + pos.Y * regionBounds.Width];
                    
            b.AppendLine($"Grid Cell: {pos}");
            var firstElementIdx = t;
            while (!firstElementIdx.IsEmpty)
            {
                var elementNode = elementNodes[firstElementIdx];
                var elementIndex = elementNode.ElementIndex;
                if (parentRef.TryGetDebugData(elementIndex, out var elementData) &&
                    parentRef.TryGetBounds(elementIndex, out var elementBounds))
                {
                    b.AppendLine($"  ${elementIndex}: {elementBounds} -> {elementData}");
                }
                else
                {
                    b.AppendLine($"  ${elementIndex}: No data available");
                }

                firstElementIdx = elementNode.NextElementNodeIndex;
            }
        }

        return b.ToString();
    }

    void QueryCellElement(in Position2D tx, 
                          BufferList<FreeListIndex> result, 
                          bool[] resultDeduplicator, FreeListIndex skipElement)
    {
        var x = tx.X - regionBounds.X;
        var y = tx.Y - regionBounds.Y;
        var firstElementIdx = elementGrid[x + y * regionBounds.Width]; 

        if (firstElementIdx == FreeListIndex.Empty)
        {
            return;
        }

        while (!firstElementIdx.IsEmpty)
        {
            var elementNode = elementNodes[firstElementIdx];
            var elementIndex = elementNode.ElementIndex;
            if (!resultDeduplicator[elementIndex.Value] &&
                elementIndex != skipElement)
            {
                result.Add(elementIndex);
                resultDeduplicator[elementIndex.Value] = true;
            }

            firstElementIdx = elementNode.NextElementNodeIndex;
        }
    }

    // Represents an element node in the quadtree.
    readonly struct GridElementNode : ISmartFreeListElement<GridElementNode>
    {
        // Points to the next element in the leaf node. A value of -1 
        // indicates the end of the list. This points to elements in QuadTree::elementNodes.
        public readonly FreeListIndex NextElementNodeIndex;

        // Stores the index to the actual data element (held in QuadTree::elements).
        public readonly FreeListIndex ElementIndex;

        public GridElementNode(FreeListIndex nextElementNodeIndex, FreeListIndex elementIndex)
        {
            NextElementNodeIndex = nextElementNodeIndex;
            ElementIndex = elementIndex;
        }

        public FreeListIndex FreePointer => ElementIndex.IsEmpty ? NextElementNodeIndex : FreeListIndex.Empty;

        public GridElementNode AsFreePointer(FreeListIndex ptr)
        {
            return new GridElementNode(ptr, default);
        }
    }
}