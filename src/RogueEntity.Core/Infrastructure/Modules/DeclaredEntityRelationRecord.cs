using System;
using System.Collections;
using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Modules
{
    public readonly struct DeclaredEntityRelationRecord : IEnumerable<Type>
    {
        static readonly ReadOnlyListWrapper<EntityRelation> Empty = new List<EntityRelation>();

        public readonly Type Subject;
        readonly Dictionary<Type, List<EntityRelation>> records;

        public DeclaredEntityRelationRecord(Type subject)
        {
            this.Subject = subject;
            this.records = new Dictionary<Type, List<EntityRelation>>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return records.Keys.GetEnumerator();
        }

        public bool TryGetRelationById(string id, out EntityRelation r)
        {
            foreach (var l in records.Values)
            {
                foreach (var ll in l)
                {
                    if (ll.Id == id)
                    {
                        r = ll;
                        return true;
                    }
                }
            }

            r = default;
            return false;
        }
        
        public ReadOnlyListWrapper<EntityRelation> this[Type t]
        {
            get
            {
                if (records.TryGetValue(t, out var rr))
                {
                    return rr;
                }

                return Empty;
            }
        }

        public bool TryGet(Type obj, out ReadOnlyListWrapper<EntityRelation> r)
        {
            if (records.TryGetValue(obj, out var rr))
            {
                r = rr;
                return true;
            }

            r = default;
            return false;
        }

        public void Declare(Type type, in EntityRelation entityRelation)
        {
            if (!records.TryGetValue(type, out var rr))
            {
                rr = new List<EntityRelation>();
                records[type] = rr;
            }

            rr.Add(entityRelation);
        }
    }
}