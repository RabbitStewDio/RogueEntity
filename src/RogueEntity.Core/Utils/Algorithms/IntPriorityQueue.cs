using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils.Algorithms
{
    public class IntPriorityQueue
    {
        internal readonly struct Node
        {
            public readonly int Data;
            public readonly float Priority;

            public Node(int data, float priority)
            {
                this.Data = data;
                this.Priority = priority;
            }

            public override string ToString()
            {
                return $"{nameof(Priority)}: {Priority}, {nameof(Data)}: {Data}";
            }
        }

        sealed class PriorityRelationalComparer : IComparer<Node>
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
        }

        static IComparer<Node> PriorityComparer { get; } = new PriorityRelationalComparer();

        internal class IntBinaryHeap : BinaryHeap<Node>
        {
            /// <summary>
            ///  Stores the index into the underlying heap for each int element.
            ///  Same as a Dictionary[int,int], but less overhead. Index values
            ///  are stored as +1 offsets to take advantage of C#'s default initialization.
            /// </summary>
            int[] forwardIndex;

            public IntBinaryHeap(int numberOfElements) : base(numberOfElements, PriorityComparer)
            {
                forwardIndex = new int[numberOfElements];
            }

            void AddToIndex(int node, int baseEntryIndex)
            {
                if (node >= forwardIndex.Length)
                {
                    Array.Resize(ref forwardIndex, node + 1);
                }

                forwardIndex[node] = baseEntryIndex;
            }

            protected override void NodeAdded(in Node node, int entryIndex)
            {
                AddToIndex(node.Data, entryIndex + 1);
            }

            protected override void NodeRemoved(in Node node)
            {
                forwardIndex[node.Data] = 0;
            }

            public override void Clear()
            {
                base.Clear();
                Array.Clear(forwardIndex, 0, forwardIndex.Length);
            }

            protected override void Swap(int parentIndex, int bubbleIndex)
            {
                // save the old values ..
                var parentVal = Data[parentIndex];
                var bubbleVal = Data[bubbleIndex];
                // do the actual swap
                base.Swap(parentIndex, bubbleIndex);
                // write back those values
                forwardIndex[parentVal.Data] = bubbleIndex + 1;
                forwardIndex[bubbleVal.Data] = parentIndex + 1;
            }

            public void RemoveElement(int node)
            {
                if (node >= forwardIndex.Length ||
                    forwardIndex[node] <= 0)
                {
                    return;
                }

                base.RemoveRaw(forwardIndex[node] - 1);
            }

            public bool Contains(int node)
            {
                if (node >= forwardIndex.Length)
                {
                    return false;
                }

                return forwardIndex[node] > 0;
            }
        }

        readonly IntBinaryHeap heap; 

        public IntPriorityQueue(int capacity)
        {
            this.heap = new IntBinaryHeap(capacity);
        }

        public int Count => heap.Size;

        public void Enqueue(int data, float priority)
        {
            heap.Add(new Node(data, priority));
        }

        public int Dequeue()
        {
            var node = heap.Remove();
            return node.Data;
        }

        public void Remove(int node)
        {
            heap.RemoveElement(node);
        }

        public void UpdatePriority(int node, float priority)
        {
            Remove(node);
            Enqueue(node, priority);
        }

        public void Clear()
        {
            heap.Clear();
        }

        public bool Contains(int node)
        {
            return heap.Contains(node);
        }

        public void Resize(int maxSize)
        {
            heap.Resize(maxSize);
        }
    }
}