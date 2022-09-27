using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Meta.StatusEffects
{
    public class StatusFlagRegistry: IEnumerable<StatusFlag>
    {
        readonly Dictionary<string, StatusFlag> statusEffectsById;
        readonly List<StatusFlag> statusEffectsByIndex;

        public StatusFlagRegistry()
        {
            statusEffectsById = new Dictionary<string, StatusFlag>();
            statusEffectsByIndex = new List<StatusFlag>();
        }

        public StatusFlagSet Create() => new StatusFlagSet(this, 0);

        public StatusFlag Reference(string id)
        {
            lock (statusEffectsById)
            {
                if (statusEffectsById.TryGetValue(id, out var existing))
                {
                    return existing;
                }

                var damageType = new StatusFlag(this, id, statusEffectsById.Count);
                statusEffectsById[id] = damageType;
                statusEffectsByIndex.Add(damageType);
                return damageType;
            }
        }

        public Dictionary<string, StatusFlag>.ValueCollection.Enumerator GetEnumerator()
        {
            // Enumerator should protect itself against concurrent modifications.
            // ReSharper disable once InconsistentlySynchronizedField
            return statusEffectsById.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<StatusFlag> IEnumerable<StatusFlag>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public StatusFlagQuery PrepareQuery(IEnumerable<StatusFlag> effects)
        {
            var mask = 0L;
            foreach (var effect in effects)
            {
                if (effect.Owner != this)
                {
                    throw new ArgumentException();
                }

                mask |= 1L << effect.LinearIndex;
            }
            return new StatusFlagQuery(mask);
        }

        public bool TryGet(string id, out StatusFlag t)
        {
            lock (statusEffectsById)
            {
                return statusEffectsById.TryGetValue(id, out t);
            }
        }

        public bool TryGet(int index, [MaybeNullWhen(false)] out StatusFlag t)
        {
            lock (statusEffectsById)
            {
                if (index < 0 || index >= statusEffectsByIndex.Count)
                {
                    t = default;
                    return false;
                }

                t = statusEffectsByIndex[index];
                return true;
            }
        }
    }
}