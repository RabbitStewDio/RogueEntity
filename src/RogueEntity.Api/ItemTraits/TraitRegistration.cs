using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RogueEntity.Api.Utils;

namespace RogueEntity.Api.ItemTraits
{
    /// <summary>
    ///   Encapsulates a cached lookup of traits for actors and items. 
    /// </summary>
    /// <typeparam name="TTrait"></typeparam>
    public class TraitRegistration<TTrait> : IEnumerable<TTrait> where TTrait: ITrait
    {
        readonly IComparer<TTrait> comparer;
        readonly List<TTrait> traits;
        readonly ConcurrentDictionary<Type, object> cachedByType;

        public TraitRegistration(IComparer<TTrait> comparer)
        {
            this.comparer = comparer;
            traits = new List<TTrait>(20);
            cachedByType = new ConcurrentDictionary<Type, object>();
        }

        bool FindById(ItemTraitId id, out TTrait trait)
        {
            foreach (var x in traits)
            {
                if (x.Id == id)
                {
                    trait = x;
                    return true;
                }
            }

            trait = default;
            return false;
        }

        public void Add(TTrait trait)
        {
            if (FindById(trait.Id, out var existing))
            {
                if (existing.Priority <= trait.Priority)
                {
                    return;
                }

                traits.Remove(existing);
            }
            traits.Add(trait);
            traits.Sort(comparer);
            cachedByType.Clear();
        }

        public void Remove<TTraitImpl>()
        {
            for (var i = traits.Count - 1; i >= 0; i--)
            {
                var t = traits[i];
                if (t is TTraitImpl)
                {
                    traits.RemoveAt(i);
                }
            }
            traits.Sort(comparer);
            cachedByType.Clear();
        }

        public bool TryQuery<TTraitImpl>(out TTraitImpl t) 
        {
            var cacheKey = typeof(TTraitImpl);
            if (cachedByType.TryGetValue(cacheKey, out var existing))
            {
                t = (TTraitImpl) existing;
                return true;
            }

            foreach (var tRaw in traits)
            {
                if (tRaw is TTraitImpl tCast)
                {
                    t = tCast;
                    cachedByType[cacheKey] = tCast;
                    return true;
                }
            }

            t = default;
            return false;
        }

        public List<TTrait>.Enumerator GetEnumerator()
        {
            return traits.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<TTrait> IEnumerable<TTrait>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public BufferList<TTraitImpl> QueryAll<TTraitImpl>(BufferList<TTraitImpl> cache)
        {
            cache = BufferList.PrepareBuffer(cache);
            foreach (var t in traits)
            {
                if (t is TTraitImpl tt)
                {
                    cache.Add(tt);
                }
            }
            return cache;
        }
    }
}