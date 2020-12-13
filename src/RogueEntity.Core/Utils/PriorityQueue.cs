using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils
{
    public class PriorityQueue<TWeight, TPayLoad> 
        where TWeight: IComparable<TWeight>
        where TPayLoad: IEquatable<TPayLoad>
    {
        readonly struct Node: IComparable<Node>
        {
            public readonly TPayLoad Data;
            public readonly TWeight Priority;

            public Node(in TPayLoad data, in TWeight priority)
            {
                this.Data = data;
                this.Priority = priority;
            }

            public override string ToString()
            {
                return $"{nameof(Priority)}: {Priority}, {nameof(Data)}: {Data}";
            }

            public int CompareTo(Node other)
            {
                return Priority.CompareTo(other.Priority);
            }

            sealed class PriorityRelationalComparer : IComparer<Node>
            {
                public int Compare(Node x, Node y)
                {
                    return x.Priority.CompareTo(y.Priority);
                }
            }

            public static IComparer<Node> PriorityComparer { get; } = new PriorityRelationalComparer();
        }

        readonly BinaryHeap<Node> heap; 

        public PriorityQueue(int capacity)
        {
            this.heap = new BinaryHeap<Node>(capacity, Node.PriorityComparer);
        }

        public int Count => heap.Size;

        public void Enqueue(in TPayLoad data, in TWeight priority)
        {
            heap.Add(new Node(data, priority));
        }

        public TPayLoad Dequeue()
        {
            var node = heap.Remove();
            return node.Data;
        }

        public bool Remove(in TPayLoad element)
        {
            var size = heap.Size;
            for (int i = 0; i < size; i += 1)
            {
                var payload = heap[i].Data;
                if (element.Equals(payload))
                {
                    heap.Remove(i);
                    return true;
                }
            }

            return false;
        }

        public void UpdatePriority(in TPayLoad node, in TWeight priority)
        { 
            // Remove(node);
            Enqueue(node, priority);
        }

        public void Clear()
        {
            heap.Clear();
        }

        public bool Contains(in TPayLoad node)
        {
            var heapSize = heap.Size;
            for (int i = 0; i < heapSize; i += 1)
            {
                if (node.Equals(heap[i].Data))
                {
                    return true;
                }
            }

            return false;
        }

        public void Resize(int maxSize)
        {
            heap.Resize(maxSize);
        }

        public int Capacity => heap.Capacity;
    }
}