using EnTTSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RogueEntity.Core.Utils
{
    public static class LinqMultiDimensionExtensions
    {
        public static IEnumerable<T> ToLinq<T>(this T[,] data)
        {
            foreach (var t in data)
            {
                yield return t;
            }
        }

        static readonly ConcurrentDictionary<string, Regex> cached = new ConcurrentDictionary<string, Regex>();

        public static bool MatchGlob(this string str, string pattern)
        {
            // Unity has a problem with creating Regex objects. 
            // Without caching, this code below is incredible slow.

            if (!cached.TryGetValue(pattern, out var r))
            {
                var globToRegEx = Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".");
                r = new Regex($"^{globToRegEx}$", RegexOptions.Compiled);
                cached[pattern] = r;
            }

            return r.IsMatch(str);
        }

        public static Optional<T> MaybeMax<T>(this IEnumerable<T> e)
        {
            var cmp = Comparer<T>.Default;
            using var en = e.GetEnumerator();
            if (!en.MoveNext())
            {
                return Optional.Empty();
            }

            var value = en.Current;
            while (en.MoveNext())
            {
                var maybeHigher = en.Current;
                if (cmp.Compare(maybeHigher, value) > 0)
                {
                    value = maybeHigher;
                }
            }

            return value;
        } 
        
        public static Optional<T> MaybeMin<T>(this IEnumerable<T> e)
        {
            var cmp = Comparer<T>.Default;
            using var en = e.GetEnumerator();
            if (!en.MoveNext())
            {
                return Optional.Empty();
            }

            var value = en.Current;
            while (en.MoveNext())
            {
                var maybeHigher = en.Current;
                if (cmp.Compare(maybeHigher, value) < 0)
                {
                    value = maybeHigher;
                }
            }

            return value;
        } 
    }
}