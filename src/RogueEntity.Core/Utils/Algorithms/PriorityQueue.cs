using System;

namespace RogueEntity.Core.Utils.Algorithms
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
        }

        readonly BinaryHeap<Node> heap; 

        public PriorityQueue(int capacity)
        {
            this.heap = new BinaryHeap<Node>(capacity);
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

        public bool Remove(in TPayLoad node)
        {
            for (int i = 0; i < heap.Size; i += 1)
            {
                if (node.Equals(heap[i].Data))
                {
                    heap.Remove(i);
                    return true;
                }
            }

            return false;
        }

        public void UpdatePriority(in TPayLoad node, in TWeight priority)
        {
            Remove(node);
            Enqueue(node, priority);
        }

        public void Clear()
        {
            heap.Clear();
        }

        public bool Contains(in TPayLoad node)
        {
            for (int i = 0; i < heap.Size; i += 1)
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
    }
}