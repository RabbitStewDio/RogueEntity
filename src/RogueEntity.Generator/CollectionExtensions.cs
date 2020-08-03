using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RogueEntity.Generator
{
    static class CollectionExtensions
    {
        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this Dictionary<TKey, TValue> d)
        {
            return new ReadOnlyDictionary<TKey, TValue>(d);
        }
    }
}
