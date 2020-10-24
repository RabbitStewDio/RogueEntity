using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils.Algorithms
{
    public class PriorityQueue<TNode> where TNode: IComparable<TNode>
    {
        readonly struct Node
        {
            public readonly TNode Data;
            public readonly float Priority;

            public Node(TNode data, float priority)
            {
                this.Data = data;
                this.Priority = priority;
            }

            public override string ToString()
            {
                return $"{nameof(Priority)}: {Priority}, {nameof(Data)}: {Data}";
            }
        }

        sealed class PriorityRelationalComparer : IComparer<BinaryHeapMap<TNode, Node>.MapEntry>
        {
            public int Compare(Node x, Node y)
            {
                var cmp = x.Priority.CompareTo(y.Priority);
                if (cmp != 0)
                {
                    return cmp;
                }

                return x.Data.CompareTo(y.Data);
            }

            public int Compare(BinaryHeapMap<TNode, Node>.MapEntry x, BinaryHeapMap<TNode, Node>.MapEntry y)
            {
                return Compare(x.value, y.value);
            }
        }

        static IComparer<BinaryHeapMap<TNode, Node>.MapEntry> PriorityComparer { get; } = new PriorityRelationalComparer();
        
        readonly BinaryHeapMap<TNode, Node> heap;

        public PriorityQueue(int capacity)
        {
            this.heap = new BinaryHeapMap<TNode, Node>(capacity, PriorityComparer);
        }

        public int Count => heap.Size;

        public void Enqueue(in TNode data, float priority)
        {
            var node = new Node(data, priority);
            heap.Put(data, node);
        }

        public TNode Dequeue()
        {
            var node = heap.Remove();
            return node.Data;
        }

        public void Remove(in TNode node)
        {
            heap.RemoveElement(node);
        }

        public void UpdatePriority(in TNode node, float priority)
        {
            Remove(node);
            Enqueue(node, priority);
        }

        public void Clear()
        {
            heap.Clear();
        }

        public bool Contains(in TNode node)
        {
            return heap.Contains(node);
        }

        public void Resize(int maxSize)
        {
            heap.Resize(maxSize);
        }
    }
}