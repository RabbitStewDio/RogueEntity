using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RogueEntity.Api.Utils
{
    public class BufferList<T> : IReadOnlyList<T>
    {
        T[] data;
        int version;

        public BufferList(int initialSize = 10)
        {
            data = new T[Math.Max(0, initialSize)];
        }

        public int Capacity
        {
            get
            {
                return data.Length;
            }
            set
            {
                if (data.Length != value)
                {
                    Array.Resize(ref data, value);
                    version += 1;
                }
            }
        }

        public void StoreAt(int index, in T input)
        {
            EnsureCapacityAutoGrowth(index + 1);
            this.data[index] = input;
            this.Count = Math.Max(index + 1, Count);
            this.version += 1;
        }

        public int Version
        {
            get { return version; }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T[] Data => data;

        public ref T TryGetRef(int index, ref T defaultValue, out bool success)
        {
            if (index < 0 || index >= Count)
            {
                success = false;
                return ref defaultValue;
            }

            success = true;
            return ref data[index];
        }

        public ref T GetRef(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            return ref data[index];
        }

        public int Count
        {
            get;
            private set;
        }

        public T this[int index]
        {
            get { return data[index]; }
            set
            {
                StoreAt(index, in value);
            }
        }

        public void Swap(int src, int dst)
        {
            (data[src], data[dst]) = (data[dst], data[src]);

            this.version += 1;
        }

        public void Clear()
        {
            if (Count > 0)
            {
                Array.Clear(data, 0, Count);
                Count = 0;
            }

            this.version += 1;
        }

        public ref T ReferenceOf(int index)
        {
            EnsureCapacityAutoGrowth(index + 1);
            return ref data[index];
        }

        public bool TryGet(int index, [MaybeNullWhen(false)] out T output)
        {
            if (index >= 0 && index < Count)
            {
                output = data[index];
                return true;
            }

            output = default;
            return false;
        }

        public void EnsureCapacity(int sizeNeeded)
        {
            if (sizeNeeded <= Capacity)
            {
                return;
            }

            Capacity = sizeNeeded;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EnsureCapacityAutoGrowth(int sizeNeeded)
        {
            if (sizeNeeded <= Capacity)
            {
                return;
            }

            var capacityDynamic = Math.Min(sizeNeeded + 10000, Capacity * 150 / 100);
            var capacityStatic = Count + 500;
            Capacity = Math.Max(capacityStatic, capacityDynamic);
        }

        public void Add(in T e)
        {
            EnsureCapacityAutoGrowth(Count + 1);

            this.data[Count] = e;
            Count += 1;
            this.version += 1;
        }

        public void RemoveLast()
        {
            this.data[Count - 1] = default!;
            this.Count -= 1;
            this.version += 1;
        }

        public void Reverse()
        {
            Array.Reverse(data, 0, Count);
        }

        public void CopyTo(List<T> target)
        {
            target.Capacity = Math.Max(target.Capacity, this.Count);
            for (var index = 0; index < this.Count; index++)
            {
                target.Add(this[index]);
            }
        }
        
        public void CopyToBuffer(BufferList<T> target)
        {
            target.Capacity = Math.Max(target.Capacity, this.Count);
            for (var index = 0; index < this.Count; index++)
            {
                target.Add(this[index]);
            }
        }
        
        public struct Enumerator : IEnumerator<T>
        {
            readonly BufferList<T> contents;
            readonly int versionAtStart;
            int index;
            T current;

            internal Enumerator(BufferList<T> widget) : this()
            {
                this.contents = widget;
                this.versionAtStart = widget.version;
                index = -1;
                current = default!;
            }

            public void Dispose()
            { }

            public bool MoveNext()
            {
                if (versionAtStart != contents.version)
                {
                    throw new InvalidOperationException("Concurrent Modification of RawList while iterating.");
                }

                if (index + 1 < contents.Count)
                {
                    index += 1;
                    current = contents[index];
                    return true;
                }

                current = default!;
                return false;
            }

            public void Reset()
            {
                index = -1;
                current = default!;
            }

            object IEnumerator.Current => Current!;

            public T Current
            {
                get
                {
                    if (index < 0 || index >= contents.Count)
                    {
                        throw new InvalidOperationException();
                    }

                    return current;
                }
            }
        }
    }

    public static class BufferList
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BufferList<T> PrepareBuffer<T>(BufferList<T>? buffer)
        {
            if (buffer == null)
            {
                return new BufferList<T>();
            }
            buffer.Clear();
            return buffer;
        }

        public static void EnsureSizeNullable<T>(this BufferList<T?> buffer, int size)
        {
            buffer.EnsureCapacity(size);
            while (buffer.Count < size)
            {
                buffer.Add(default);
            }
        }

        public static void EnsureSizeStruct<T>(this BufferList<T> buffer, int size)
            where T: struct
        {
            buffer.EnsureCapacity(size);
            while (buffer.Count < size)
            {
                buffer.Add(default);
            }
        }
    }
}
