using System;
using System.Collections.Generic;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemRegistry<TContext, TItemId> : IItemRegistry where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly ILogger logger = SLog.ForContext<ItemRegistry<TContext, TItemId>>();

        readonly Func<IBulkItemDeclaration<TContext, TItemId>, int, TItemId> bulkItemIdFactory;
        readonly Dictionary<ItemDeclarationId, (int, IBulkItemDeclaration<TContext, TItemId>)> bulkItems;
        readonly Dictionary<int, IBulkItemDeclaration<TContext, TItemId>> bulkItemReverseIndex;
        readonly Dictionary<ItemDeclarationId, IItemDeclaration> itemsById;
        readonly Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TContext, TItemId>> referenceItemsById;
        readonly List<IItemDeclaration> items;
        int bulkItemIdSequence;

        public ItemRegistry(Func<IBulkItemDeclaration<TContext, TItemId>, int, TItemId> bulkItemIdFactory)
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
                return bulkItemIdFactory(item, id);
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

        public void Register(IReferenceItemDeclaration<TContext, TItemId> itemDeclaration)
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
        }

        public void Register(IBulkItemDeclaration<TContext, TItemId> item)
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
                id = reg.Item1;
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
                item = reg.Item2;
                return true;
            }

            item = default;
            return false;
        }

        public bool TryGetReferenceItemById(ItemDeclarationId id, out IReferenceItemDeclaration<TContext, TItemId> item)
        {
            return referenceItemsById.TryGetValue(id, out item);
        }
    }
}