using System.Collections.Generic;

namespace RogueEntity.Api.Utils
{
    public static class ApiExtensions
    {
        internal static bool EqualsList<TItem>(ReadOnlyListWrapper<TItem> a, ReadOnlyListWrapper<TItem> b)
        {
            if (a.Count != b.Count) return false;
            
            for (var i = 0; i < a.Count; i++)
            {
                if (!EqualityComparer<TItem>.Default.Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool EqualsList<TItem>(IReadOnlyList<TItem>? a, IReadOnlyList<TItem>? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            
            for (var i = 0; i < a.Count; i++)
            {
                if (!EqualityComparer<TItem>.Default.Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }
        
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }
    }
}