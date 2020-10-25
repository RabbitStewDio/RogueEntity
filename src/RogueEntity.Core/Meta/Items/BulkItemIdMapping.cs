using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MessagePack;

namespace RogueEntity.Core.Meta.Items
{
    /// <summary>
    ///   A serializable mapping of internal bulk-item IDs to more stable item-declaration-ids (aka item-names).
    ///   This is intended to be stored along with any serialized game state to allow the system to map old item-ids
    ///   back to valid local ids. This in return allows save games to survive minor additions of content or changes
    ///   in the order of ids generated. 
    /// </summary>
    [MessagePackObject]
    [DataContract]
    [Serializable]
    public class BulkItemIdMapping : IBulkItemIdMapping
    {
        [Key(0)]
        [DataMember(Order = 0)]
        readonly Dictionary<int, ItemDeclarationId> itemById;

        public BulkItemIdMapping()
        {
            this.itemById = new Dictionary<int, ItemDeclarationId>();
        }

        public BulkItemIdMapping(IBulkItemIdMapping copy): this()
        {
            foreach (var c in copy)
            {
                if (copy.TryResolveBulkItem(c, out var id))
                {
                    itemById[c] = id;
                }
            }
        }

        public void Register(int itemIndex, ItemDeclarationId itemName)
        {
            itemById.Add(itemIndex, itemName);
        }

        public bool TryResolveBulkItem(int id, out ItemDeclarationId itemName)
        {
            return itemById.TryGetValue(id, out itemName);
        }

        public Dictionary<int, ItemDeclarationId>.KeyCollection.Enumerator GetEnumerator() => itemById.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}