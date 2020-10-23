using System;
using System.Collections;
using System.Collections.Generic;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemRegistry<TContext, TItemId> : IItemRegistry, 
                                                   IBulkItemIdMapping 
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly ILogger logger = SLog.ForContext<ItemRegistry<TContext, TItemId>>();

        readonly Func<int, TItemId> bulkItemIdFactory;
        readonly Dictionary<ItemDeclarationId, (int index, IBulkItemDeclaration<TContext, TItemId> itemDeclaration)> bulkItems;
        readonly Dictionary<int, IBulkItemDeclaration<TContext, TItemId>> bulkItemReverseIndex;
        readonly Dictionary<ItemDeclarationId, IItemDeclaration> itemsById;
        readonly Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TContext, TItemId>> referenceItemsById;
        readonly List<IItemDeclaration> items;
        int bulkItemIdSequence;

        public ItemRegistry(Func<int, TItemId> bulkItemIdFactory)
        {
            this.bulkItemIdFactory = bulkItemIdFactory;
            bulkItems = new Dictionary<ItemDeclarationId, (int, IBulkItemDeclaration<TContext, TItemId>)>();
            bulkItemReverseIndex = new Dictionary<int, IBulkItemDeclaration<TContext, TItemId>>();
            referenceItemsById = new Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TContext, TItemId>>();
            itemsById = new Dictionary<ItemDeclarationId, IItemDeclaration>();
            items = new List<IItemDeclaration>();
            bulkItemIdSequence = 1;
        }

        public TItemId GenerateBulkItemId(IBulkItemDeclaration<TContext,TItemId> item)
        {
            if (TryGetBulkItemId(item, out var id))
            {
                return bulkItemIdFactory(id);
            }
            throw new ArgumentException($"The given item declaration {item.Id} has not been registered here.");
        }

        public ReadOnlyListWrapper<IItemDeclaration> Items
        {
            get
            {
                if (items.Count != itemsById.Count)
                {
                    items.Clear();
                    items.AddRange(itemsById.Values);
                }

                return items;
            }
        }

        public ItemDeclarationId Register(IReferenceItemDeclaration<TContext, TItemId> itemDeclaration)
        {
            if (itemsById.ContainsKey(itemDeclaration.Id))
            {
                logger.Information("Redeclaration of existing item {ItemId}", itemDeclaration.Id);
                itemsById[itemDeclaration.Id] = itemDeclaration;
                referenceItemsById[itemDeclaration.Id] = itemDeclaration;
            }
            else
            {
                itemsById.Add(itemDeclaration.Id, itemDeclaration);
                referenceItemsById.Add(itemDeclaration.Id, itemDeclaration);
            }

            if (bulkItems.TryGetValue(itemDeclaration.Id, out var reg))
            {
                var (internalId, _) = reg;
                bulkItems.Remove(itemDeclaration.Id);
                bulkItemReverseIndex.Remove(internalId);
            }

            items.Clear();
            return itemDeclaration.Id;
        }

        public ItemDeclarationId Register(IBulkItemDeclaration<TContext, TItemId> item)
        {
            if (itemsById.ContainsKey(item.Id))
            {
                logger.Information("Redeclaration of existing item {ItemId}", item.Id);
                itemsById[item.Id] = item;
            }
            else
            {
                itemsById.Add(item.Id, item);
            }

            referenceItemsById.Remove(item.Id);

            if (bulkItems.TryGetValue(item.Id, out var reg))
            {
                var (iid, _) = reg;
                bulkItems[item.Id] = (iid, item);
                bulkItemReverseIndex[iid] = item;
            }
            else
            {
                var internalId = (ushort)bulkItemIdSequence;
                bulkItemIdSequence += 1;
                bulkItems.Add(item.Id, (internalId, item));
                bulkItemReverseIndex.Add(internalId, item);
            }

            items.Clear();

            return item.Id;
        }

        public IItemDeclaration ReferenceItemById(ItemDeclarationId id)
        {
            if (TryGetItemById(id, out var item))
            {
                return item;
            }

            throw new ArgumentException($"Item '{id}' does not exist in this item registry.");
        }

        public bool TryGetBulkItemId(IBulkItemDeclaration<TContext, TItemId> item, out int id)
        {
            if (bulkItems.TryGetValue(item.Id, out var reg))
            {
                id = reg.index;
                return true;
            }

            id = default;
            return false;
        }

        public bool TryResolveBulkItem(TItemId bulkIndex, out IBulkItemDeclaration<TContext, TItemId> item)
        {
            return bulkItemReverseIndex.TryGetValue(bulkIndex.BulkItemId, out item);
        }

        public bool TryGetItemById(ItemDeclarationId id, out IItemDeclaration item)
        {
            return itemsById.TryGetValue(id, out item);
        }

        public bool TryGetBulkItemById(ItemDeclarationId id, out IBulkItemDeclaration<TContext, TItemId> item)
        {
            if (bulkItems.TryGetValue(id, out var reg))
            {
                item = reg.itemDeclaration;
                return true;
            }

            item = default;
            return false;
        }

        public bool TryGetReferenceItemById(ItemDeclarationId id, out IReferenceItemDeclaration<TContext, TItemId> item)
        {
            return referenceItemsById.TryGetValue(id, out item);
        }

        public IBulkItemIdMapping BulkItemMapping => this;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<int> GetEnumerator()
        {
            return bulkItemReverseIndex.Keys.GetEnumerator();
        }

        public bool TryResolveBulkItem(int id, out ItemDeclarationId itemName)
        {
            if (bulkItemReverseIndex.TryGetValue(id, out var val))
            {
                itemName = val.Id;
                return true;
            }

            itemName = default;
            return false;
        }
    }
}