using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RogueEntity.Core.Utils.Algorithms
{
    public class BinaryHeapMap<K, V>
    {
        #region HelperClasses
        internal struct MapEntry
        {
            public readonly K key;
            public readonly V value;

            public MapEntry(K key, V value)
            {
                this.key = key;
                this.value = value;
            }

            public override string ToString()
            {
                return string.Format("[MapEntry: key={0}, value={1}]", key, value);
            }
        }

        class MapKeyComparer : IComparer<MapEntry>
        {
            readonly IComparer<K> keyComparer;

            public MapKeyComparer(IComparer<K> parent)
            {
                this.keyComparer = parent ?? throw new ArgumentNullException();
            }

            public int Compare(MapEntry first, MapEntry second)
            {
                return keyComparer.Compare(first.key, second.key);
            }
        }

        class MapValueComparer : IComparer<MapEntry>
        {
            readonly IComparer<V> keyComparer;

            public MapValueComparer(IComparer<V> parent)
            {
                if (parent == null)
                {
                    throw new ArgumentNullException();
                }
                this.keyComparer = parent;
            }

            public int Compare(MapEntry first, MapEntry second)
            {
                return keyComparer.Compare(first.value, second.value);
            }
        }

        class IndexedBinaryHeap : BinaryHeap<MapEntry>
        {
            readonly Dictionary<K, int> indexData;

            public IndexedBinaryHeap(int numberOfElements, IComparer<MapEntry> comparer) : base(numberOfElements, comparer)
            {
                indexData = new Dictionary<K, int>(numberOfElements);
            }

            protected override void Swap(int parentIndex, int bubbleIndex)
            {
                base.Swap(parentIndex, bubbleIndex);
                var parentNode = this.Data[parentIndex].key;
                var bubbleNode = this.Data[bubbleIndex].key;

                indexData[parentNode] = parentIndex;
                indexData[bubbleNode] = bubbleIndex;
            }

            protected override void NodeAdded(in MapEntry node, int entryIndex)
            {
                this.indexData.Add(node.key, entryIndex);
            }

            protected override void NodeRemoved(in MapEntry node)
            {
                this.indexData.Remove(node.key);
            }

            public override void Add(in MapEntry node)
            {
                var idx = IndexOf(node.key);
                if (idx != -1)
                {
                    Remove(idx);
                }

                base.Add(node);
            }

            public int IndexOf(K key)
            {
                if (indexData.TryGetValue(key, out var value))
                {
                    return value;
                }
                return -1;
            }

            public K[] Keys()
            {
                var keys = new K[indexData.Count];
                indexData.Keys.CopyTo(keys, 0);
                return keys;
            }

            public override void Clear()
            {
                base.Clear();
                indexData.Clear();
            }

            public override string ToString()
            {
                return "[IndexBinaryHeap=" + base.ToString() + "; " + indexData + "]";
            }

            public bool TryGetValue(K key, out V value)
            {
                var index = IndexOf(key);
                if (index == -1)
                {
                    value = default(V);
                    return false;
                }

                value = Data[index].value;
                return true;

            }

            public void Revalidate()
            {
                var keys = Keys();
                for (var i = 0; i < keys.Length; i += 1)
                {
                    var value = IndexOf(keys[i]);
                    var storedKey = Data[value].key;
                    if (keys[i].Equals(storedKey) == false)
                    {
                        Debug.WriteLine("Inconsistent: " + keys[i] + " -> " + storedKey);
                        Debug.WriteLine("Inconsistent index: " + this);
                    }

                }
            }
        }
        #endregion

        readonly IndexedBinaryHeap backend;

        internal BinaryHeapMap(int numberOfElements, IComparer<MapEntry> comparer)
        {
            backend = new IndexedBinaryHeap(numberOfElements, comparer);
        }

        public static BinaryHeapMap<K, V> CreateMapByKey(int numberOfElements, IComparer<K> keyComparer)
        {
            if (keyComparer == null)
            {
                throw new ArgumentNullException();
            }
            return new BinaryHeapMap<K, V>(numberOfElements, new MapKeyComparer(keyComparer));
        }

        public static BinaryHeapMap<K, V> CreateMapByValue(int numberOfElements, IComparer<V> keyComparer)
        {
            if (keyComparer == null)
            {
                throw new ArgumentNullException();
            }
            return new BinaryHeapMap<K, V>(numberOfElements, new MapValueComparer(keyComparer));
        }

        public void Put(K key, V value)
        {
            backend.Add(new MapEntry(key, value));
#if DEBUG_VALION
      backend.Revalidate ();
#endif
        }

        public void RemoveElement(K key)
        {
            var keyIdx = backend.IndexOf(key);
            if (keyIdx == -1)
            {
                return;
            }

            backend.RemoveRaw(keyIdx);
        }

        public V Remove()
        {
            var m = backend.Remove();
#if DEBUG_VALION
      backend.Revalidate ();
#endif
            return m.value;
        }

        public bool Contains(in K key) => backend.IndexOf(key) != -1;

        public bool TryGet(K key, out V value)
        {
            return backend.TryGetValue(key, out value);
        }

        public void Clear()
        {
            backend.Clear();
        }

        public K[] Keys()
        {
            return backend.Keys();
        }

        public override string ToString()
        {
            return string.Format("[BinaryHeapMap: Size={0}, Backend={1}]", Size, backend);
        }

        public void Resize(int maxSize)
        {
            backend.Resize(maxSize);
        }

        public int Size => backend.Size;
    }
}