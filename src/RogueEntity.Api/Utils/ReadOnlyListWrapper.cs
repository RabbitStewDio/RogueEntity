using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueEntity.Api.Utils
{
    public readonly struct ReadOnlyListWrapper<T> : IReadOnlyList<T>, ICollection<T>
    {
        static readonly EqualityComparer<T> EqualityComparer = EqualityComparer<T>.Default;
        public static readonly ReadOnlyListWrapper<T> Empty = new List<T>();
        static readonly List<T> EmptyList = new List<T>();
        readonly IReadOnlyList<T> list;

        public ReadOnlyListWrapper(IReadOnlyList<T> list)
        {
            this.list = list ?? EmptyList;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(object value)
        {
            return ((IList)list).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)list).IndexOf(value);
        }

        public bool Contains(T item)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (EqualityComparer.Equals(list[i], item))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Exists(Predicate<T> match)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (match(list[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public T Find(Predicate<T> match)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (match(list[i]))
                {
                    return list[i];
                }
            }

            return default;
        }

        public ReadOnlyListWrapper<T> FindAll(Predicate<T> match)
        {
            List<T> retval = new List<T>();
            for (var i = 0; i < list.Count; i++)
            {
                if (match(list[i]))
                {
                    retval.Add(list[i]);
                }
            }

            return retval;
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            for (var i = startIndex; i < count; i++)
            {
                if (match(list[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return FindIndex(startIndex, Count - startIndex, match);
        }

        public int FindIndex(Predicate<T> match)
        {
            return FindIndex(0, Count, match);
        }

        public T FindLast(Predicate<T> match)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                {
                    return list[i];
                }
            }

            return default;

        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            for (var i = count - 1; i >= startIndex; i--)
            {
                if (match(list[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return FindLastIndex(startIndex, Count - startIndex, match);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return FindLastIndex(0, Count, match);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this.list);
        }

        public struct Enumerator : IEnumerator<T>
        {
            readonly IReadOnlyList<T> contents;
            int index;
            T current;

            internal Enumerator(IReadOnlyList<T> widget) : this()
            {
                this.contents = widget;
                index = -1;
                current = default;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index + 1 < contents.Count)
                {
                    index += 1;
                    current = contents[index];
                    return true;
                }

                current = default;
                return false;
            }

            public void Reset()
            {
                index = -1;
                current = default;
            }

            object IEnumerator.Current => Current;

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

        public int IndexOf(T item)
        {
            return IndexOf(item,0, Count);
        }

        public int IndexOf(T item, int index)
        {
            return IndexOf(item, index, Count - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            for (var i = index; i < count; i++)
            {
                if (EqualityComparer.Equals(list[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        public int LastIndexOf(T item)
        {
            return LastIndexOf(item, 0, Count);
        }

        public int LastIndexOf(T item, int index)
        {
            return LastIndexOf(item, index, Count - index);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            for (var i = count - 1; i >= index; i--)
            {
                if (EqualityComparer.Equals(list[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        public T[] ToArray()
        {
            return list.ToArray();
        }

        public bool TrueForAll(Predicate<T> match)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (!match(list[i]))
                {
                    return false;
                }
            }

            return true;

        }

        public int Count => list.Count;

        public T this[int index]
        {
            get => list[index];
        }

        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException();
        }

        void ICollection<T>.Clear()
        {
            throw new InvalidOperationException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            for (var i = 0; i < list.Count; i++)
            {
                array[i + arrayIndex] = list[i];
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException();
        }

        bool ICollection<T>.IsReadOnly => true;

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("[");
            foreach (var i in this)
            {
                if (b.Length > 1)
                {
                    b.Append(", ");
                }

                b.Append(i);
            }
            b.Append("]");
            return b.ToString();
        }

        public static implicit operator ReadOnlyListWrapper<T>(List<T> raw) => new ReadOnlyListWrapper<T>(raw);
        public static implicit operator ReadOnlyListWrapper<T>(BufferList<T> raw) => new ReadOnlyListWrapper<T>(raw);
    }

}