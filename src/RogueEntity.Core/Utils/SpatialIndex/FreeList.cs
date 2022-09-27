using System;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public class FreeList<T>
    {
        readonly int growth;
        T[] elementData;
        /// <summary>
        ///   Pointer to the next free element if an element had been previously occupied.
        ///
        ///   Contains zero if the element has never been occupied. Contains -1 if the
        ///   position is currently filled.
        /// 
        ///   The contents of this index are stored as index+1 so that we dont have to manually
        ///   fill in -1 on all free fields.
        /// </summary>
        int[] freeIndex;

        int firstFreeElement;
        int count;

        public FreeList(int initialSize = 128) : this(initialSize, initialSize) { }

        public FreeList(int initialSize, int growth)
        {
            initialSize = Math.Max(0, initialSize);
            this.growth = Math.Max(1, growth);
            firstFreeElement = -1;
            elementData = new T[initialSize];
            freeIndex = new int[initialSize];
        }

        public bool IsEmpty => count == 0;

        public FreeListIndex Add(in T element)
        {
            if (firstFreeElement != -1)
            {
                var index = firstFreeElement;
                firstFreeElement = freeIndex[index] - 1;
                freeIndex[index] = -1;

                elementData[index] = element;
                count += 1;
                return FreeListIndex.Of(index);
            }

            if (elementData.Length == count)
            {
                // expand size if needed
                Array.Resize(ref elementData, count + growth);
                Array.Resize(ref freeIndex, count + growth);
            }

            var insertIndex = count;
            elementData[insertIndex] = element;
            freeIndex[insertIndex] = -1;
            count += 1;
            return FreeListIndex.Of(insertIndex);
        }

        public void Remove(FreeListIndex idx)
        {
            var index = idx.Value;
            if (index < 0 || index >= Range)
            {
                throw new IndexOutOfRangeException();
            }

            if (freeIndex[index] != -1)
            {
                throw new ArgumentException("This index position is not occupied");
            }

            elementData[index] = default!;
            freeIndex[index] = firstFreeElement + 1;
            firstFreeElement = index;
            count -= 1;
        }

        public void Replace(FreeListIndex idx, T data)
        {
            var index = idx.Value;
            if (index < 0 || index >= Range)
            {
                throw new IndexOutOfRangeException();
            }

            if (freeIndex[index] != -1)
            {
                throw new ArgumentException("This index position is not occupied");
            }

            this.elementData[index] = data;
        }

        public void Clear()
        {
            Array.Clear(elementData, 0, elementData.Length);
            Array.Clear(freeIndex, 0, elementData.Length);
            firstFreeElement = -1;
            count = 0;
        }

        public int Range => elementData.Length;

        public bool TryGetValue(FreeListIndex idx, out T data)
        {
            var index = idx.Value;
            if (index < 0 || index >= Range)
            {
                data = default!;
                return false;
            }

            if (freeIndex[index] != -1)
            {
                data = default!;
                return false;
            }

            data = elementData[index];
            return true;
        }
        
        public T this[FreeListIndex idx]
        {
            get
            {
                var index = idx.Value;
                if (index < 0 || index >= Range)
                {
                    throw new IndexOutOfRangeException();
                }

                if (freeIndex[index] != -1)
                {
                    throw new ArgumentException("This index position is not occupied");
                }

                return elementData[index];
            }
        }
    }
}
