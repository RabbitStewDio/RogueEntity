using JetBrains.Annotations;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public class QuadTreeCore<TSpatialIndexAdapter>
        where TSpatialIndexAdapter : IQuadTreeAdapter
    {
        static readonly FreeListIndex RootElement = FreeListIndex.Of(0);
        static readonly ThreadLocal<Stack<QuadNodeData>> ProcessingStackHolder = new ThreadLocal<Stack<QuadNodeData>>(() => new Stack<QuadNodeData>());
        readonly TSpatialIndexAdapter adapter;
        readonly FreeList<QuadElementNode> elementNodes;
        readonly FreeList<QuadNode> nodes;
        readonly int maxDepth;
        readonly int maxElements;
        readonly AABB boundingBox;

        public QuadTreeCore([NotNull] TSpatialIndexAdapter adapter, AABB boundingBox, int maxElements, int maxDepth)
        {
            this.maxDepth = Math.Max(1, maxDepth);
            this.maxElements = Math.Max(1, maxElements);
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.boundingBox = boundingBox;

            elementNodes = new FreeList<QuadElementNode>();
            nodes = new FreeList<QuadNode>();
            nodes.Add(QuadNode.Leaf());
        }

        public void InsertElement(FreeListIndex data, in BoundingBox bounds)
        {
            InsertNode(RootElement, 0, boundingBox, data, bounds);
        }

        public void RemoveElement(FreeListIndex elementIndex, BoundingBox elementBounds)
        {
            var removeHandler = new RemoveVisitor(this, elementIndex);
            ProcessLeaves(RootElement, 0, boundingBox, elementBounds, removeHandler);
        }

        public void CleanUp()
        {
            if (nodes[RootElement].IsLeaf)
            {
                return;
            }

            var toProcess = new Stack<FreeListIndex>();
            toProcess.Push(RootElement);

            while (toProcess.Count > 0)
            {
                var nodeIndex = toProcess.Pop();
                var firstChild = nodes[nodeIndex].FirstChildIdx;
                var emptyLeafCount = 0;

                for (var childIndex = 0; childIndex < 4; childIndex += 1)
                {
                    var child = firstChild + childIndex;

                    // Increment empty leaf count if the child is an empty 
                    // leaf. Otherwise if the child is a branch, add it to
                    // the stack to be processed in the next iteration.
                    if (nodes[child].IsLeaf)
                    {
                        if (nodes[child].ChildCount == 0)
                        {
                            emptyLeafCount += 1;
                        }
                    }
                    else
                    {
                        toProcess.Push(child);
                    }
                }

                // If all the children were empty leaves, remove them and 
                // make this node the new empty leaf.
                if (emptyLeafCount == 4)
                {
                    // Remove all 4 children in reverse order so that they 
                    // can be reclaimed on subsequent insertions in proper
                    // order.
                    nodes.Remove(firstChild + 3);
                    nodes.Remove(firstChild + 2);
                    nodes.Remove(firstChild + 1);
                    nodes.Remove(firstChild + 0);

                    nodes.Replace(nodeIndex, QuadNode.Leaf());
                }
            }
        }

        public List<int> Query(in BoundingBox bb, List<int> result, int skipElement = -1)
        {
            var resultDeduplicator = ArrayPool<bool>.Shared.Rent(adapter.ElementIndexRange);
            try
            {
                return Query(in bb, result, skipElement);
            }
            finally
            {
                ArrayPool<bool>.Shared.Return(resultDeduplicator);
            }
        }

        public List<FreeListIndex> Query(in BoundingBox bb, List<FreeListIndex> result, bool[] deduplicator, FreeListIndex skipElement = default)
        {
            var x = new CollectQueryVisitor(this, result ?? new List<FreeListIndex>(), bb, deduplicator, skipElement);
            ProcessLeaves(RootElement, 0, boundingBox, bb, x);
            return x.ResultCollector;
        }

        void ProcessLeaves<TVisitor>(in TVisitor v)
            where TVisitor : ILeafNodeVisitor
        {
            ProcessLeaves(RootElement, 0, boundingBox, boundingBox, v);
        }

        void ProcessLeaves<TVisitor>(FreeListIndex node, int depth, in AABB searchSpace, in BoundingBox boundingBox, in TVisitor visitor)
            where TVisitor : ILeafNodeVisitor
        {
            var nodesToProcess = ProcessingStackHolder.Value;

            nodesToProcess.Push(new QuadNodeData(node, depth, searchSpace));

            while (nodesToProcess.Count > 0)
            {
                var n = nodesToProcess.Pop();
                if (nodes[n.Index].IsLeaf)
                {
                    visitor.ProcessLeafNode(n);
                    continue;
                }

                visitor.ProcessBranchNode(n);

                // if branch, first child index points to an element in the nodes collection.
                var firstChildIndex = nodes[n.Index].FirstChildIdx;
                var (nodeCenterX, nodeCenterY, nodeExtendX, nodeExtendY) = n.NodeBounds;
                var sx = nodeExtendX / 2;
                var sy = nodeExtendY / 2;
                var left = nodeCenterX - sx;
                var top = nodeCenterY - sy;
                var right = nodeCenterX + sx;
                var bottom = nodeCenterY + sy;

                if (boundingBox.Top <= nodeCenterY)
                {
                    if (boundingBox.Left <= nodeCenterX)
                    {
                        nodesToProcess.Push(new QuadNodeData(firstChildIndex, n.Depth + 1, new AABB(left, top, sx, sy)));
                    }

                    if (boundingBox.Right > nodeCenterX)
                    {
                        nodesToProcess.Push(new QuadNodeData(firstChildIndex + 1, n.Depth + 1, new AABB(right, top, sx, sy)));
                    }
                }

                if (boundingBox.Bottom > nodeCenterY)
                {
                    if (boundingBox.Left <= nodeCenterX)
                    {
                        nodesToProcess.Push(new QuadNodeData(firstChildIndex + 2, n.Depth + 1, new AABB(left, bottom, sx, sy)));
                    }

                    if (boundingBox.Right > nodeCenterX)
                    {
                        nodesToProcess.Push(new QuadNodeData(firstChildIndex + 3, n.Depth + 1, new AABB(right, bottom, sx, sy)));
                    }
                }
            }
        }

        void ProcessLeavesShallow<TVisitor>(FreeListIndex node, int depth, in AABB searchSpace, in BoundingBox boundingBox, in TVisitor v)
            where TVisitor : ILeafNodeVisitor
        {
            // if branch, first child index points to an element in the nodes collection.
            var firstChildIndex = nodes[node].FirstChildIdx;
            var (nodeCenterX, nodeCenterY, nodeExtendX, nodeExtendY) = searchSpace;
            var sx = nodeExtendX / 2;
            var sy = nodeExtendY / 2;
            var left = nodeCenterX - sx;
            var top = nodeCenterY - sy;
            var right = nodeCenterX + sx;
            var bottom = nodeCenterY + sy;

            if (boundingBox.Top <= nodeCenterY)
            {
                if (boundingBox.Left <= nodeCenterX)
                {
                    v.ProcessLeafNode(new QuadNodeData(firstChildIndex, depth + 1, new AABB(left, top, sx, sy)));
                }

                if (boundingBox.Right > nodeCenterX)
                {
                    v.ProcessLeafNode(new QuadNodeData(firstChildIndex + 1, depth + 1, new AABB(right, top, sx, sy)));
                }
            }

            if (boundingBox.Bottom > nodeCenterY)
            {
                if (boundingBox.Left <= nodeCenterX)
                {
                    v.ProcessLeafNode(new QuadNodeData(firstChildIndex + 2, depth + 1, new AABB(left, bottom, sx, sy)));
                }

                if (boundingBox.Right > nodeCenterX)
                {
                    v.ProcessLeafNode(new QuadNodeData(firstChildIndex + 3, depth + 1, new AABB(right, bottom, sx, sy)));
                }
            }
        }

        void InsertNode(FreeListIndex index, int depth, in AABB bb, FreeListIndex element, in BoundingBox elementBounds)
        {
            var v = new CollectLeafNodesVisitor(this, element);
            ProcessLeaves(index, depth, bb, elementBounds, v);
        }

        void InsertLeaf(in QuadNodeData dt, FreeListIndex element)
        {
            var node = nodes[dt.Index];
            var elementNodeIndex = elementNodes.Add(new QuadElementNode(node.FirstChildIdx, element));
            nodes.Replace(dt.Index, node.AddChildToLeaf(elementNodeIndex));

            if (node.ChildCount == maxElements && dt.Depth < maxDepth)
            {
                var elementsInLeaves = CollectElementFromLeafNode(elementNodeIndex);

                var fc = nodes.Add(QuadNode.Leaf());
                nodes.Add(QuadNode.Leaf());
                nodes.Add(QuadNode.Leaf());
                nodes.Add(QuadNode.Leaf());

                nodes.Replace(dt.Index, QuadNode.Branch(fc));

                foreach (var elementIndex in elementsInLeaves)
                {
                    if (!adapter.TryGetBounds(elementIndex, out var bb))
                    {
                        continue;
                    }

                    var v = new CollectLeafNodesVisitor(this, elementIndex);
                    ProcessLeavesShallow(dt.Index, dt.Depth, dt.NodeBounds, bb, v);
                }
            }
        }

        // Note: This method may be called recursively, so we have to return a new
        // list each time this method is called. The list's life should hopefully be
        // short lived and thus not bother the garbage collector too much. 
        List<FreeListIndex> CollectElementFromLeafNode(FreeListIndex nfc)
        {
            var elementIndices = new List<FreeListIndex>();
            while (!nfc.IsEmpty)
            {
                var elementNode = elementNodes[nfc];
                elementNodes.Remove(nfc);
                elementIndices.Add(elementNode.ElementIndex);

                nfc = elementNode.NextElementNodeIndex;
            }

            return elementIndices;
        }

        public string Print()
        {
            var printer = PrintNodesVisitor.Construct(this, new StringBuilder());
            ProcessLeaves(printer);
            return printer.ToString();
        }

        public void PrintInto([NotNull] StringBuilder b)
        {
            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            var printer = PrintNodesVisitor.Construct(this, b);
            ProcessLeaves(printer);
        }

        interface ILeafNodeVisitor
        {
            void ProcessLeafNode(in QuadNodeData data);
            void ProcessBranchNode(in QuadNodeData data);
        }

        readonly struct RemoveVisitor : ILeafNodeVisitor
        {
            readonly QuadTreeCore<TSpatialIndexAdapter> self;
            readonly FreeListIndex elementToRemove;

            public RemoveVisitor(QuadTreeCore<TSpatialIndexAdapter> self, FreeListIndex elementToRemove)
            {
                this.self = self;
                this.elementToRemove = elementToRemove;
            }

            public void ProcessLeafNode(in QuadNodeData leaf)
            {
                var nodeIndex = leaf.Index;
                var node = self.nodes[nodeIndex];
                var elementNodeIndex = node.FirstChildIdx;
                var prevElementNodeIndex = FreeListIndex.Empty;
                while (!elementNodeIndex.IsEmpty &&
                       self.elementNodes[elementNodeIndex].ElementIndex != elementToRemove)
                {
                    prevElementNodeIndex = elementNodeIndex;
                    elementNodeIndex = self.elementNodes[elementNodeIndex].NextElementNodeIndex;
                }

                if (!elementNodeIndex.IsEmpty)
                {
                    var nextIndex = self.elementNodes[elementNodeIndex].NextElementNodeIndex;
                    if (prevElementNodeIndex.IsEmpty)
                    {
                        // first element in the linked list
                        self.nodes.Replace(nodeIndex, node.RemoveChildFromLeaf(nextIndex));
                        self.elementNodes.Remove(nextIndex);
                    }
                    else
                    {
                        // middle or end, so we restitch the linked list around the removed element
                        var prevNode = self.elementNodes[prevElementNodeIndex];
                        self.elementNodes.Replace(prevElementNodeIndex, new QuadElementNode(nextIndex, prevNode.ElementIndex));
                        self.elementNodes.Remove(nextIndex);
                        self.nodes.Replace(nodeIndex, node.DecrementChildCount());
                    }
                }
            }

            public void ProcessBranchNode(in QuadNodeData data)
            { }
        }

        readonly struct CollectQueryVisitor : ILeafNodeVisitor
        {
            readonly QuadTreeCore<TSpatialIndexAdapter> tree;
            readonly bool[] resultDeduplicator;
            readonly BoundingBox bb;
            readonly FreeListIndex skipElement;
            public readonly List<FreeListIndex> ResultCollector;

            public CollectQueryVisitor(QuadTreeCore<TSpatialIndexAdapter> tree,
                                       List<FreeListIndex> resultCollector,
                                       in BoundingBox bb,
                                       bool[] resultDeduplicator,
                                       FreeListIndex skipElement = default)
            {
                if (resultDeduplicator.Length < tree.adapter.ElementIndexRange)
                {
                    throw new ArgumentException();
                }

                this.tree = tree;
                this.ResultCollector = resultCollector;
                this.resultDeduplicator = resultDeduplicator;
                Array.Clear(resultDeduplicator, 0, tree.adapter.ElementIndexRange);
                this.bb = bb;
                this.skipElement = skipElement;
            }

            public void ProcessLeafNode(in QuadNodeData data)
            {
                var nodeIndex = data.Index;
                var elementNodeIndex = tree.nodes[nodeIndex].FirstChildIdx;
                while (!elementNodeIndex.IsEmpty)
                {
                    var elementNode = tree.elementNodes[elementNodeIndex];
                    var elementIndex = elementNode.ElementIndex;
                    if (!resultDeduplicator[elementIndex.Value] &&
                        elementIndex != skipElement)
                    {
                        if (tree.adapter.TryGetBounds(elementIndex, out var elementBounds) &&
                            elementBounds.Intersects(bb))
                        {
                            ResultCollector.Add(elementIndex);
                            resultDeduplicator[elementIndex.Value] = true;
                        }
                    }

                    elementNodeIndex = elementNode.NextElementNodeIndex;
                }
            }

            public void ProcessBranchNode(in QuadNodeData data)
            { }
        }

        readonly struct CollectLeafNodesVisitor : ILeafNodeVisitor
        {
            readonly QuadTreeCore<TSpatialIndexAdapter> self;
            readonly FreeListIndex element;

            public CollectLeafNodesVisitor(QuadTreeCore<TSpatialIndexAdapter> self, FreeListIndex element)
            {
                this.self = self;
                this.element = element;
            }

            public void ProcessLeafNode(in QuadNodeData data)
            {
                self.InsertLeaf(data, element);
            }

            public void ProcessBranchNode(in QuadNodeData data)
            { }
        }

        readonly struct PrintNodesVisitor : ILeafNodeVisitor
        {
            readonly StringBuilder b;
            readonly QuadTreeCore<TSpatialIndexAdapter> tree;

            PrintNodesVisitor(QuadTreeCore<TSpatialIndexAdapter> tree, StringBuilder b)
            {
                this.tree = tree;
                this.b = b;
            }

            public void ProcessLeafNode(in QuadNodeData data)
            {
                var elementNodeIndex = tree.nodes[data.Index].FirstChildIdx;
                if (elementNodeIndex.IsEmpty)
                {
                    AppendIndent(data.Depth);
                    b.AppendLine("- L:<Empty>");
                    return;
                }

                AppendIndent(data.Depth);
                b.AppendLine("- L");

                while (!elementNodeIndex.IsEmpty)
                {
                    var node = tree.elementNodes[elementNodeIndex];

                    AppendIndent(data.Depth);
                    b.Append("  * ");
                    b.Append(node.ElementIndex);
                    b.Append(" -> ");
                    if (tree.adapter.TryGetBounds(node.ElementIndex, out var bounds))
                    {
                        b.Append(bounds);
                    }
                    else
                    {
                        b.Append("*ERROR*");
                    }

                    b.Append(" -> ");

                    if (tree.adapter.TryGetDebugData(node.ElementIndex, out var debugData))
                    {
                        b.Append(debugData);
                    }
                    else
                    {
                        b.Append("*ERROR*");
                    }

                    b.AppendLine();

                    elementNodeIndex = node.NextElementNodeIndex;
                }
            }

            public void ProcessBranchNode(in QuadNodeData data)
            {
                AppendIndent(data.Depth);
                b.Append(" - B -> ");
                b.Append(data.NodeBounds);
                b.AppendLine();
            }

            void AppendIndent(int depth)
            {
                for (int d = 0; d < depth; d += 1)
                {
                    b.Append("  ");
                }
            }

            public override string ToString()
            {
                return b.ToString();
            }

            public static PrintNodesVisitor Construct(QuadTreeCore<TSpatialIndexAdapter> t, StringBuilder b)
            {
                return new PrintNodesVisitor(t, b);
            }
        }
    }

    // Represents an element node in the quadtree.
    readonly struct QuadElementNode : ISmartFreeListElement<QuadElementNode>
    {
        public FreeListIndex FreePointer => ElementIndex.IsEmpty ? NextElementNodeIndex : FreeListIndex.Empty;

        public QuadElementNode AsFreePointer(FreeListIndex ptr)
        {
            return new QuadElementNode(ptr, FreeListIndex.Empty);
        }

        // Points to the next element in the leaf node. A value of -1 
        // indicates the end of the list. This points to elements in QuadTree::elementNodes.
        public readonly FreeListIndex NextElementNodeIndex;

        // Stores the index to the actual data element (held in QuadTree::elements).
        public readonly FreeListIndex ElementIndex;

        public QuadElementNode(FreeListIndex nextElementNodeIndex, FreeListIndex elementIndex)
        {
            NextElementNodeIndex = nextElementNodeIndex;
            ElementIndex = elementIndex;
        }
    }

    readonly struct QuadNodeData
    {
        public readonly AABB NodeBounds;

        /// <summary>
        ///   A pointer into QuadTree::Nodes.
        /// </summary>
        public readonly FreeListIndex Index;

        public readonly int Depth;

        public QuadNodeData(FreeListIndex index, int depth, in AABB nodeBounds)
        {
            NodeBounds = nodeBounds;
            Index = index;
            Depth = depth;
        }
    }

    internal readonly struct QuadNode : ISmartFreeListElement<QuadNode>
    {
        public FreeListIndex FreePointer => ChildCount == -1 ? FirstChildIdx : FreeListIndex.Empty;

        public QuadNode AsFreePointer(FreeListIndex ptr)
        {
            return new QuadNode(ptr, -1);
        }

        /// <summary>
        ///   Contains either the index of the first child node if this node is a branch node,
        ///   or the index of the data element stored in the tree.
        /// </summary>
        public readonly FreeListIndex FirstChildIdx;

        public readonly int ChildCount;

        QuadNode(FreeListIndex firstChildIdx, int childCount)
        {
            FirstChildIdx = firstChildIdx;
            ChildCount = childCount;
        }

        public bool IsLeaf => ChildCount >= 0;

        public static QuadNode Leaf() => new QuadNode(FreeListIndex.Empty, 0);
        public static QuadNode Branch(FreeListIndex childIndex) => new QuadNode(childIndex, -1);

        public QuadNode AddChildToLeaf(FreeListIndex elementNodeIndex)
        {
            return new QuadNode(elementNodeIndex, ChildCount + 1);
        }

        public QuadNode RemoveChildFromLeaf(FreeListIndex elementNodeIndex)
        {
            return new QuadNode(elementNodeIndex, ChildCount - 1);
        }

        public QuadNode DecrementChildCount()
        {
            return new QuadNode(FirstChildIdx, ChildCount - 1);
        }
    }
}
