using System;
using System.Collections.Generic;
using System.Collections;

namespace RogueEntity.Core.Utils
{
    /// <summary>
    /// Circular buffer.
    /// 
    /// When writing to a full buffer:
    /// PushBack -> removes this[0] / Front()
    /// PushFront -> removes this[Size-1] / Back()
    /// 
    /// this implementation is inspired by
    /// http://www.boost.org/doc/libs/1_53_0/libs/circular_buffer/doc/circular_buffer.html
    /// because I liked their interface.
    ///
    /// Originally published at  https://github.com/joaoportela/CircullarBuffer-CSharp
    /// Licensed under the "Unlicense" License.
    /// </summary>
    public class CircularBuffer<T> : IReadOnlyList<T>
    {
        static readonly EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

        T[] buffer;

        /// <summary>
        /// The _start. Index of the first element in buffer.
        /// </summary>
        int start;

        /// <summary>
        /// The _end. Index after the last element in the buffer.
        /// </summary>
        int end;

        /// <summary>
        /// The _size. Buffer size.
        /// </summary>
        int count;

        public CircularBuffer(int capacity, int autoCapacityIncrement = 0)
        {
            if (capacity < 1)
            {
                throw new ArgumentException(
                    "Circular buffer cannot have negative or zero capacity.", nameof(capacity));
            }

            buffer = new T[capacity];
            start = 0;
            end = 0;
            AutoExpandCapacityCount = Math.Max(0, autoCapacityIncrement);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
        /// 
        /// </summary>
        /// <param name='capacity'>
        /// Buffer capacity. Must be positive.
        /// </param>
        /// <param name='items'>
        /// Items to fill buffer with. Items length must be less than capacity.
        /// Suggestion: use Skip(x).Take(y).ToArray() to build this argument from
        /// any enumerable.
        /// </param>
        /// <param name="autoCapacityIncrement"></param>
        public CircularBuffer(int capacity, T[] items, int autoCapacityIncrement = 0)
        {
            if (capacity < 1)
            {
                throw new ArgumentException(
                    "Circular buffer cannot have negative or zero capacity.", nameof(capacity));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (items.Length > capacity)
            {
                throw new ArgumentException(
                    "Too many items to fit circular buffer", nameof(items));
            }

            buffer = new T[capacity];

            Array.Copy(items, buffer, items.Length);
            count = items.Length;

            start = 0;
            end = count == capacity ? 0 : count;

            AutoExpandCapacityCount = Math.Max(0, autoCapacityIncrement);
        }

        /// <summary>
        /// Maximum capacity of the buffer. Elements pushed into the buffer after
        /// maximum capacity is reached (IsFull = true), will remove an element.
        /// </summary>
        public int Capacity
        {
            get
            {
                return buffer.Length;
            }
            set
            {
                if (value < count)
                {
                    throw new ArgumentException("New capacity must larger or equal to the number of elements in this buffer");
                }

                if (value == buffer.Length)
                {
                    return;
                }

                var newBuffer = new T[value];
                CopyTo(newBuffer);
                buffer = newBuffer;
                start = 0;
                end = count == value ? 0 : count;
            }
        }

        public int IndexOf(T value)
        {
            for (var i = 0; i < count; i++)
            {
                var m = this[i];
                if (equalityComparer.Equals(m, value))
                {
                    return i;
                }
            }

            return -1;
        }

        public T RemoveAt(int index)
        {
            if (index < 0)
            {
                throw new ArgumentException();
            }
            
            if (index >= count)
            {
                throw new ArgumentException();
            }

            if (index == 0)
            {
                // special case: First element removed. This can be solved by just adjusting pointers.
                return PopFront();
            }

            if (index == count - 1)
            {
                // special case: Last element removed. This can be solved by just adjusting pointers.
                return PopBack();
            }
            

            var actualIndex = InternalIndex(index);
            var value = this[index];
            if (start < end)
            {
                // normal single linear index.
                Array.Copy(buffer, actualIndex + 1, buffer, actualIndex, count - index - 1);
                buffer[end] = default!;
                end -= 1;
                return value;
            }

            // split buffer case
            if (actualIndex >= start)
            {
                // actual index is in the first segment (after start, before end of array)
                Array.Copy(buffer, actualIndex + 1, buffer, actualIndex, buffer.Length - index - 1);
                buffer[buffer.Length - 1] = buffer[0];
                Array.Copy(buffer, 1, buffer, 0, end - 1);
            }
            else
            {
                Array.Copy(buffer, actualIndex + 1, buffer, actualIndex, end - actualIndex - 1);
            }
            buffer[end] = default!;
            end -= 1;
            return value;
        }

        public int AutoExpandCapacityCount { get; set; }
        public bool AutoExpandCapacity => AutoExpandCapacityCount > 0;

        public bool IsFull => count == Capacity;

        public bool IsEmpty => count == 0;

        /// <summary>
        /// Current buffer size (the number of elements that the buffer has).
        /// </summary>
        public int Count => count;

        /// <summary>
        /// Element at the front of the buffer - this[0].
        /// </summary>
        /// <returns>The value of the element of type T at the front of the buffer.</returns>
        public T Front()
        {
            ThrowIfEmpty();
            return buffer[start];
        }

        /// <summary>
        /// Element at the back of the buffer - this[Size - 1].
        /// </summary>
        /// <returns>The value of the element of type T at the back of the buffer.</returns>
        public T Back()
        {
            ThrowIfEmpty();
            return buffer[(end != 0 ? end : Capacity) - 1];
        }

        public T this[int index]
        {
            get
            {
                if (IsEmpty)
                {
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
                }

                if (index >= count)
                {
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {count}");
                }

                int actualIndex = InternalIndex(index);
                return buffer[actualIndex];
            }
            set
            {
                if (IsEmpty)
                {
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
                }

                if (index >= count)
                {
                    throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {count}");
                }

                int actualIndex = InternalIndex(index);
                buffer[actualIndex] = value;
            }
        }

        /// <summary>
        /// Pushes a new element to the back of the buffer. Back()/this[Size-1]
        /// will now return this element.
        /// 
        /// When the buffer is full, the element at Front()/this[0] will be 
        /// popped to allow for this new element to fit.
        /// </summary>
        /// <param name="item">Item to push to the back of the buffer</param>
        public void PushBack(T item)
        {
            if (IsFull)
            {
                if (!AutoExpandCapacity)
                {
                    buffer[end] = item;
                    Increment(ref end);
                    start = end;
                    return;
                }

                Capacity += AutoExpandCapacityCount;
            }

            buffer[end] = item;
            Increment(ref end);
            ++count;
        }

        /// <summary>
        /// Pushes a new element to the front of the buffer. Front()/this[0]
        /// will now return this element.
        /// 
        /// When the buffer is full, the element at Back()/this[Size-1] will be 
        /// popped to allow for this new element to fit.
        /// </summary>
        /// <param name="item">Item to push to the front of the buffer</param>
        public void PushFront(T item)
        {
            if (IsFull)
            {
                if (!AutoExpandCapacity)
                {
                    Decrement(ref start);
                    end = start;
                    buffer[start] = item;
                    return;
                }

                // expand capacity.
                Capacity += AutoExpandCapacityCount;
            }

            Decrement(ref start);
            buffer[start] = item;
            ++count;
        }

        public T PopBack()
        {
            var retval = Back();
            SkipBack();
            return retval;
        }
        
        public bool TryPopBack(out T value)
        {
            if (IsEmpty)
            {
                value = default!;
                return false;
            }

            value = PopBack();
            return true;
        }

        /// <summary>
        /// Removes the element at the back of the buffer. Decreasing the 
        /// Buffer size by 1.
        /// </summary>
        public void SkipBack()
        {
            ThrowIfEmpty("Cannot take elements from an empty buffer.");
            Decrement(ref end);
            buffer[end] = default!;
            --count;
        }

        public bool TryPopFront(out T value)
        {
            if (IsEmpty)
            {
                value = default!;
                return false;
            }

            value = PopFront();
            return true;
        }

        public T PopFront()
        {
            var retval = Front();
            SkipFront();
            return retval;
        }
        
        /// <summary>
        /// Removes the element at the front of the buffer. Decreasing the 
        /// Buffer size by 1.
        /// </summary>
        public void SkipFront()
        {
            ThrowIfEmpty("Cannot take elements from an empty buffer.");
            buffer[start] = default!;
            Increment(ref start);
            --count;
        }

        /// <summary>
        /// Copies the buffer contents to an array, according to the logical
        /// contents of the buffer (i.e. independent of the internal 
        /// order/contents)
        /// </summary>
        /// <returns>A new array with a copy of the buffer contents.</returns>
        public T[] ToArray()
        {
            T[] newArray = new T[Count];
            var s1 = ArrayOne();
            var s2 = ArrayTwo();

            
            Assert.NotNull(s1.Array);
            Assert.NotNull(s2.Array);

            Array.Copy(s1.Array, s1.Offset, newArray, 0, s1.Count);
            Array.Copy(s2.Array, s2.Offset, newArray, s1.Count, s2.Count);
            return newArray;
        }

        public void CopyTo(T[] array) => CopyTo(array, 0);
        
        public void CopyTo(T[] array, int offset)
        {
            if (array.Length < count)
            {
                throw new IndexOutOfRangeException();
            }

            var s1 = ArrayOne();
            var s2 = ArrayTwo();

            Assert.NotNull(s1.Array);
            Assert.NotNull(s2.Array);

            Array.Copy(s1.Array, s1.Offset, array, offset + 0, s1.Count);
            Array.Copy(s2.Array, s2.Offset, array, offset + s1.Count, s2.Count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var s1 = ArrayOne();
            for (int i = 0; i < s1.Count; i++)
            {
                Assert.NotNull(s1.Array);
                yield return s1.Array[s1.Offset + i];
            }

            var s2 = ArrayTwo();
            for (int i = 0; i < s2.Count; i++)
            {
                Assert.NotNull(s2.Array);
                yield return s2.Array[s2.Offset + i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ThrowIfEmpty(string message = "Cannot access an empty buffer.")
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Increments the provided index variable by one, wrapping
        /// around if necessary.
        /// </summary>
        /// <param name="index"></param>
        void Increment(ref int index)
        {
            if (++index == Capacity)
            {
                index = 0;
            }
        }

        /// <summary>
        /// Decrements the provided index variable by one, wrapping
        /// around if necessary.
        /// </summary>
        /// <param name="index"></param>
        void Decrement(ref int index)
        {
            if (index == 0)
            {
                index = Capacity;
            }

            index--;
        }

        /// <summary>
        /// Converts the index in the argument to an index in <code>_buffer</code>
        /// </summary>
        /// <returns>
        /// The transformed index.
        /// </returns>
        /// <param name='index'>
        /// External index.
        /// </param>
        int InternalIndex(int index)
        {
            return start + (index < (Capacity - start) ? index : index - Capacity);
        }

        // doing ArrayOne and ArrayTwo methods returning ArraySegment<T> as seen here: 
        // http://www.boost.org/doc/libs/1_37_0/libs/circular_buffer/doc/circular_buffer.html#classboost_1_1circular__buffer_1957cccdcb0c4ef7d80a34a990065818d
        // http://www.boost.org/doc/libs/1_37_0/libs/circular_buffer/doc/circular_buffer.html#classboost_1_1circular__buffer_1f5081a54afbc2dfc1a7fb20329df7d5b
        // should help a lot with the code.

        // The array is composed by at most two non-contiguous segments, 
        // the next two methods allow easy access to those.

        ArraySegment<T> ArrayOne()
        {
            if (IsEmpty)
            {
                return new ArraySegment<T>(buffer, 0, 0);
            }

            if (start < end)
            {
                return new ArraySegment<T>(buffer, start, end - start);
            }

            return new ArraySegment<T>(buffer, start, buffer.Length - start);
        }

        ArraySegment<T> ArrayTwo()
        {
            if (IsEmpty)
            {
                return new ArraySegment<T>(buffer, 0, 0);
            }

            if (start < end)
            {
                return new ArraySegment<T>(buffer, end, 0);
            }

            return new ArraySegment<T>(buffer, 0, end);
        }

        public void RemoveAll(T value)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (equalityComparer.Equals(this[i], value))
                {
                    RemoveAt(i);
                }
            }
        }

        public bool Contains(T value) => IndexOf(value) != -1;

    }
}
