using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.Algorithms
{
    //Binary Heap

    /// Binary heap implementation. Binary heaps are really fast for ordering
    /// nodes in a way that makes it possible to get the node with the lowest F
    /// score. Also known as a priority queue. * \see
    /// http://en.wikipedia.org/wiki/Binary_heap * \see
    /// http://weblogs.asp.net/cumpsd/archive/2005/02/13/371719.aspx *  * This
    /// class ignores the first entry in the array to simplify the mathematics
    /// used. *  * The children of any given item are always stored at the item’s
    /// location * 2 and the item’s location * 2 + 1. For example, in the image
    /// given above,  * the item with value 20 is stored at index 3 and its two
    /// children can be found at index 6 (3 * 2) and index 7 (3 * 2 + 1). */
    public class BinaryHeap<T>
    {
        protected T[] Data;
        protected readonly IComparer<T> Comparer;
        int numberOfItems;

        public BinaryHeap(int numberOfElements, 
                          IComparer<T> comparer = null)
        {
            if (numberOfElements <= 1)
            {
                throw new ArgumentException();
            }

            this.Data = new T[numberOfElements];
            this.numberOfItems = 1;
            this.Comparer = comparer ?? Comparer<T>.Default;
        }

        public int Size
        {
            get { return numberOfItems - 1; }
        }

        public virtual void HandleSizeLimitReached()
        {
            Array.Resize(ref Data, Math.Max(numberOfItems + 100, numberOfItems * 3 / 2));
        }

        protected virtual void NodeAdded(in T node, int entryIndex)
        {
        }

        protected virtual void NodeRemoved(in T node)
        {
        }

        /** Adds a node to the heap */
        public virtual void Add(in T node)
        {
            if (numberOfItems == Data.Length)
            {
                HandleSizeLimitReached();
            }

            Data[numberOfItems] = node;
            NodeAdded(in node, numberOfItems);

            var bubbleIndex = numberOfItems;

            while (bubbleIndex != 1)
            {
                var parentIndex = bubbleIndex / 2;

                if (Comparer.Compare(node, Data[parentIndex]) <= 0)
                {
                    Swap(parentIndex, bubbleIndex);
                    bubbleIndex = parentIndex;
                }
                else
                {
                    break;
                }
            }

            numberOfItems++;
            Revalidate();
        }

        public virtual void Clear()
        {
            this.numberOfItems = 1;
            for (var i = 0; i < Data.Length; i++)
            {
                Data[i] = default;
            }
        }

        /** Returns the node with the lowest F score from the heap */
        public T Remove()
        {
            return Remove(0);
        }

        public T Remove(int index)
        {
            if (numberOfItems <= 1)
            {
                throw new InvalidOperationException("Empty heap");
            }

            if (index < 0)
            {
                throw new IndexOutOfRangeException($"Index {index} is invalid.");
            }

            if (index >= Size)
            {
                throw new IndexOutOfRangeException($"Index {index} is invalid.");
            }

            return RemoveRaw(index + 1);
        }

        public T RemoveRaw(int index)
        {
            if (index < 1 || index > Size)
            {
                throw new ArgumentOutOfRangeException($"{index} must be greater than zero and less than {Size}");
            }

            numberOfItems--;

            if (index == numberOfItems)
            {
                var returnItemX = Data[index];
                Data[index] = default;
                NodeRemoved(returnItemX);
                return returnItemX;
            }

            var returnItem = Data[index];
            Data[index] = Data[numberOfItems];
            Data[numberOfItems] = default;

            // Notify of removal of old node
            NodeRemoved(returnItem);
            // Node-Swap: removal of last node
            NodeRemoved(in Data[index]);
            // Node-Swap: move last node into old node's position.
            NodeAdded(in Data[index], index);

            var swapItem = index;
            int parent;

            // removing a node: Take the first node out. Now move the last node (largest node) to the top and trickle it downwards.

            do
            {
                parent = swapItem;
                var p2 = parent * 2;
                if ((p2 + 1) < numberOfItems)
                {
                    // Both children exist
                    if (Comparer.Compare(Data[parent], Data[p2]) >= 0)
                    {
                        swapItem = p2; //2 * parent;
                    }

                    if (Comparer.Compare(Data[swapItem], Data[p2 + 1]) >= 0)
                    {
                        swapItem = p2 + 1;
                    }
                }
                else if ((p2) < numberOfItems)
                {
                    // Only one child exists
                    if (Comparer.Compare(Data[parent], Data[p2]) >= 0)
                    {
                        swapItem = p2;
                    }
                }

                // One if the parent's children are smaller or equal, swap them
                if (parent != swapItem)
                {
                    Swap(parent, swapItem);
                }
            } while (parent != swapItem);

            Revalidate();
            NodeRemoved(in returnItem);
            return returnItem;
        }

        public T this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= numberOfItems)
                {
                    throw new IndexOutOfRangeException();
                }
                
                return Data[idx + 1];
            }
        }
        
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        void Revalidate()
        {
            return;
#pragma warning disable 162
            for (var i = 2; i < numberOfItems; i += 1)
            {
                var item = Data[i];
                var parent = Data[i / 2];
                if (Comparer.Compare(parent, item) > 1)
                {
                    throw new ArgumentException("Unable to validate entry: " + i + " := " + item);
                } 
            }
#pragma warning restore 162
        }
        
        protected virtual void Swap(int parentIndex, int bubbleIndex)
        {
            var tmpValue = Data[parentIndex];
            Data[parentIndex] = Data[bubbleIndex];
            Data[bubbleIndex] = tmpValue;
        }

        /** Returns a nicely formatted string describing the tree structure. '!!!' marks after a value means that the tree is not correct at that node (i.e it should be swapped with it's parent) */
        public override string ToString()
        {
            var text = new System.Text.StringBuilder();

            text.Append("\n=== Writing Binary Heap ===\n");
            text.Append("Number of items: ").Append(numberOfItems).Append(" => Count: ").Append(numberOfItems - 1)
                .Append("\n");
            text.Append("Capacity: ").Append(Data.Length);
            text.Append("\n");
            if (numberOfItems > 1)
            {
                WriteBranch(1, 1, text);
            }

            text.Append("\n\n");
            return text.ToString();
        }

        /** Writes a branch of the tree to a StringBuilder. Used by #ToString */
        void WriteBranch(int index, int depth, System.Text.StringBuilder text)
        {
            text.Append("\n");
            for (var i = 0; i < depth; i++)
            {
                text.Append("   ");
            }

            text.Append(index);
            text.Append(":");
            text.Append(Data[index]);

            if (index > 1)
            {
                var parentIndex = index / 2;

                if (Comparer.Compare(Data[index], Data[parentIndex]) < 0)
                {
                    text.Append("  !!!");
                }
            }

            var p2 = index * 2;
            if ((p2 + 1) < numberOfItems)
            {
                // Both children exist
                WriteBranch(p2, depth + 1, text);
                WriteBranch(p2 + 1, depth + 1, text);
            }
            else if (p2 < numberOfItems)
            {
                // Only one child exists
                WriteBranch(p2, depth + 1, text);
            }
        }

        public void Resize(int maxSize)
        {
            Array.Resize(ref Data, maxSize);
        }
    }
}