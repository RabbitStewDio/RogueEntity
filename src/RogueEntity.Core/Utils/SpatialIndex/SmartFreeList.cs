using System;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    /// <summary>
    ///   A free list that merges the free-index with the data-index by giving empty data elements
    ///   an option to store free-pointer data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SmartFreeList<T>
        where T : struct, ISmartFreeListElement<T>
    {
        readonly int growth;
        T[] elementData;

        FreeListIndex firstFreeElement;
        int count;

        public SmartFreeList(int initialSize = 128) : this(initialSize, initialSize) { }

        public SmartFreeList(int initialSize, int growth)
        {
            initialSize = Math.Max(0, initialSize);
            this.growth = Math.Max(1, growth);
            firstFreeElement = FreeListIndex.Empty;
            elementData = new T[initialSize];
        }

        public int Count => count;
        public bool IsEmpty => count == 0;

        public FreeListIndex Add(in T element)
        {
            if (!firstFreeElement.IsEmpty)
            {
                var index = firstFreeElement;
                // extract pointer to next free element
                ref var tmp = ref elementData[index.Value];
                firstFreeElement = tmp.FreePointer;
                // then overwrite position with new value
                tmp = element;

                count += 1;
                // finally return insert position to caller.
                return index;
            }

            if (elementData.Length == count)
            {
                // expand size if needed
                Array.Resize(ref elementData, count + growth);
            }

            // append at the end of the data array.
            var insertIndex = count;
            elementData[insertIndex] = element;
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

            if (elementData[index].FreePointer.IsEmpty)
            {
                throw new ArgumentException("Already freed");
            }

            ref var tmp = ref elementData[index];
            tmp = tmp.AsFreePointer(firstFreeElement);
            firstFreeElement = FreeListIndex.Of(index);
            count -= 1;
        }

        public void Replace(FreeListIndex idx, T data)
        {
            var index = idx.Value;
            if (index < 0 || index >= Range)
            {
                throw new IndexOutOfRangeException();
            }

            if (elementData[index].FreePointer.IsEmpty)
            {
                throw new ArgumentException("This index position is not occupied");
            }

            this.elementData[index] = data;
        }

        public void Clear()
        {
            Array.Clear(elementData, 0, elementData.Length);
            firstFreeElement = FreeListIndex.Empty;
            count = 0;
        }

        public int Range => elementData.Length;

        public bool TryGetValue(FreeListIndex idx, out T data)
        {
            var index = idx.Value;
            if (index < 0 || index >= Range)
            {
                data = default;
                return false;
            }

            var x = elementData[index];
            if (x.FreePointer.IsEmpty)
            {
                data = default;
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

                var x = elementData[index];
                if (x.FreePointer.IsEmpty)
                {
                    throw new ArgumentException("This index position is not occupied");
                }

                return x;
            }
        }
    }
}
