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

        public int Add(in T element)
        {
            if (firstFreeElement != -1)
            {
                var index = firstFreeElement;
                firstFreeElement = freeIndex[index] - 1;
                freeIndex[index] = -1;

                elementData[index] = element;
                count += 1;
                return index;
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
            return insertIndex;
        }

        public void Remove(int index)
        {
            if (index < 0 || index >= Range)
            {
                throw new IndexOutOfRangeException();
            }

            if (freeIndex[index] != -1)
            {
                throw new ArgumentException("This index position is not occupied");
            }

            elementData[index] = default;
            freeIndex[index] = firstFreeElement + 1;
            firstFreeElement = index;
            count -= 1;
        }

        public void Replace(int index, T data)
        {
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

        public bool TryGetValue(int index, out T data)
        {
            if (index < 0 || index >= Range)
            {
                data = default;
                return false;
            }

            if (freeIndex[index] != -1)
            {
                data = default;
                return false;
            }

            data = elementData[index];
            return true;
        }
        
        public T this[int index]
        {
            get
            {
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

    /// <summary>
    ///   An internal storage hook to hold a pointer to the next free
    ///   element. Will only be used when there is no element stored.
    ///   Will always contain an negative value when valid. Implementations
    ///   should reuse a field that can never contain negative values.
    /// </summary>
    interface ISmartFreeListElement<T> where T: struct, ISmartFreeListElement<T>
    {
        int FreePointer { get; }
        T AsFreePointer(int ptr);
    }

    /// <summary>
    ///   A free list that merges the free-index with data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SmartFreeList<T> 
        where T : struct, ISmartFreeListElement<T>
    {
        readonly int growth;
        T[] elementData;

        int firstFreeElement;
        int count;

        public SmartFreeList(int initialSize = 128) : this(initialSize, initialSize) { }

        public SmartFreeList(int initialSize, int growth)
        {
            initialSize = Math.Max(0, initialSize);
            this.growth = Math.Max(1, growth);
            firstFreeElement = -1;
            elementData = new T[initialSize];
        }

        public bool IsEmpty => count == 0;

        public int Add(in T element)
        {
            if (firstFreeElement != -1)
            {
                var index = firstFreeElement;
                ref var tmp = ref elementData[index];
                firstFreeElement = -(tmp.FreePointer - 1);
                tmp = element;

                count += 1;
                return index;
            }

            if (elementData.Length == count)
            {
                // expand size if needed
                Array.Resize(ref elementData, count + growth);
            }

            var insertIndex = count;
            elementData[insertIndex] = element;
            count += 1;
            return insertIndex;
        }

        public void Remove(int index)
        {
            if (index < 0 || index >= Range)
            {
                throw new IndexOutOfRangeException();
            }

            if (elementData[index].FreePointer < 0)
            {
                throw new ArgumentException("Already freed");
            }

            ref var tmp = ref elementData[index];
            tmp = tmp.AsFreePointer(-(firstFreeElement + 1));
            firstFreeElement = index;
            count -= 1;
        }

        public void Replace(int index, T data)
        {
            if (index < 0 || index >= Range)
            {
                throw new IndexOutOfRangeException();
            }

            if (elementData[index].FreePointer < 0)
            {
                throw new ArgumentException("This index position is not occupied");
            }

            this.elementData[index] = data;
        }

        public void Clear()
        {
            Array.Clear(elementData, 0, elementData.Length);
            firstFreeElement = -1;
            count = 0;
        }

        public int Range => elementData.Length;

        public T this[int index]
        {
            get
            {
                return elementData[index];
            }
        }
    }
}
