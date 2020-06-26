using System;
using System.Collections.Generic;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public class ItemRegistry<TContext, TItemId> : IItemRegistry where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly ILogger logger = SLog.ForContext<ItemRegistry<TContext, TItemId>>();

        readonly Func<IBulkItemDeclaration<TContext, TItemId>, int, TItemId> bulkItemIdFactory;
        readonly Dictionary<ItemDeclarationId, int> bulkItems;
        readonly Dictionary<int, IBulkItemDeclaration<TContext, TItemId>> bulkItemReverseIndex;
        readonly Dictionary<ItemDeclarationId, IItemDeclaration> itemsById;
        readonly List<IItemDeclaration> items;
        int bulkItemIdSequence;

        public ItemRegistry(Func<IBulkItemDeclaration<TContext, TItemId>, int, TItemId> bulkItemIdFactory)
        {
            this.bulkItemIdFactory = bulkItemIdFactory;
            bulkItems = new Dictionary<ItemDeclarationId, int>();
            bulkItemReverseIndex = new Dictionary<int, IBulkItemDeclaration<TContext, TItemId>>();
            itemsById = new Dictionary<ItemDeclarationId, IItemDeclaration>();
            items = new List<IItemDeclaration>();
        }

        public TItemId GenerateBulkItemId(IBulkItemDeclaration<TContext,TItemId> item)
        {
            if (TryGetBulkItemId(item, out var id))
            {
                return bulkItemIdFactory(item, id);
            }
            throw new ArgumentException();
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
            }
            else
            {
                itemsById.Add(itemDeclaration.Id, itemDeclaration);
            }

            if (bulkItems.TryGetValue(itemDeclaration.Id, out var internalId))
            {
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

            if (bulkItems.TryGetValue(item.Id, out var internalId))
            {
                bulkItems[item.Id] = internalId;
                bulkItemReverseIndex[internalId] = item;
            }
            else
            {
                internalId = (ushort)bulkItemIdSequence;
                bulkItemIdSequence += 1;
                bulkItems.Add(item.Id, internalId);
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
            return bulkItems.TryGetValue(item.Id, out id);
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
            if (itemsById.TryGetValue(id, out var i) &&
                i is IBulkItemDeclaration<TContext, TItemId> bi)
            {
                item = bi;
                return true;
            }

            item = default;
            return false;
        }
    }
}