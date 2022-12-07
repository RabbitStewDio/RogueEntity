using EnTTSharp.Entities;
using System;
using System.Collections.Generic;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemRegistry<TItemId> : IItemRegistryBackend<TItemId>
        where TItemId : struct, IEntityKey
    {
        readonly IBulkDataStorageMetaData<TItemId> itemIdMetaData;
        readonly ILogger logger = SLog.ForContext<ItemRegistry<TItemId>>();

        readonly Dictionary<ItemDeclarationId, (int index, IBulkItemDeclaration<TItemId> itemDeclaration)> bulkItems;
        readonly Dictionary<int, IBulkItemDeclaration<TItemId>> bulkItemReverseIndex;
        readonly Dictionary<ItemDeclarationId, IItemDeclaration> itemsById;
        readonly Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TItemId>> referenceItemsById;
        readonly List<IItemDeclaration> items;
        int bulkItemIdSequence;

        public ItemRegistry(IBulkDataStorageMetaData<TItemId> itemIdMetaData)
        {
            this.itemIdMetaData = itemIdMetaData;
            bulkItems = new Dictionary<ItemDeclarationId, (int, IBulkItemDeclaration<TItemId>)>();
            bulkItemReverseIndex = new Dictionary<int, IBulkItemDeclaration<TItemId>>();
            referenceItemsById = new Dictionary<ItemDeclarationId, IReferenceItemDeclaration<TItemId>>();
            itemsById = new Dictionary<ItemDeclarationId, IItemDeclaration>();
            items = new List<IItemDeclaration>();
            bulkItemIdSequence = 1;
        }

        public IBulkDataStorageMetaData<TItemId> EntityMetaData => itemIdMetaData;

        public TItemId GenerateBulkItemId(IBulkItemDeclaration<TItemId> item)
        {
            if (!TryGetBulkItemId(item, out var id))
            {
                throw new ArgumentException($"The given item declaration {item.Id} has not been registered here.");
            }

            if (itemIdMetaData.TryCreateBulkKey(id, 0, out var result))
            {
                return result;
            }

            throw new ArgumentException($"The given entity type does not support bulk-entities.");
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

        public ItemDeclarationId Register(IItemDeclaration itemDeclaration)
        {
            return itemDeclaration switch
            {
                IReferenceItemDeclaration<TItemId> refDec => Register(refDec),
                IBulkItemDeclaration<TItemId> bulkDec => Register(bulkDec),
                _ => throw new ArgumentException(nameof(itemDeclaration))
            };
        }

        public ItemDeclarationId Register(IReferenceItemDeclaration<TItemId> itemDeclaration)
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

            if (bulkItems.TryGetValue(itemDeclaration.Id, out var reg))
            {
                var (internalId, _) = reg;
                bulkItems.Remove(itemDeclaration.Id);
                bulkItemReverseIndex.Remove(internalId);

                referenceItemsById.Add(itemDeclaration.Id, itemDeclaration);
            }
            else
            {
                referenceItemsById[itemDeclaration.Id] = itemDeclaration;
            }

            items.Clear();
            return itemDeclaration.Id;
        }

        public ItemDeclarationId Register(IBulkItemDeclaration<TItemId> item)
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

            if (bulkItems.TryGetValue(item.Id, out var reg))
            {
                var (iid, _) = reg;
                bulkItems[item.Id] = (iid, item);
                bulkItemReverseIndex[iid] = item;
            }
            else
            {
                if (bulkItemIdSequence >= itemIdMetaData.MaxBulkKeyTypes)
                {
                    throw new InvalidOperationException
                        ($"Unable to declare additional bulk item; limit of {itemIdMetaData.MaxBulkKeyTypes} reached");
                }

                referenceItemsById.Remove(item.Id);

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

        public bool TryGetBulkItemId(IBulkItemDeclaration<TItemId> item, out int id)
        {
            if (bulkItems.TryGetValue(item.Id, out var reg))
            {
                id = reg.index;
                return true;
            }

            id = default;
            return false;
        }

        public bool TryResolveBulkItem(TItemId bulkIndex, [MaybeNullWhen(false)] out IBulkItemDeclaration<TItemId> item)
        {
            if (itemIdMetaData.TryDeconstructBulkKey(bulkIndex, out var itemId, out _))
            {
                return bulkItemReverseIndex.TryGetValue(itemId, out item);
            }

            item = default;
            return false;
        }

        public bool TryGetItemById(ItemDeclarationId id, [MaybeNullWhen(false)] out IItemDeclaration item)
        {
            return itemsById.TryGetValue(id, out item);
        }

        public bool TryGetBulkItemById(ItemDeclarationId id, [MaybeNullWhen(false)] out IBulkItemDeclaration<TItemId> item)
        {
            if (bulkItems.TryGetValue(id, out var reg))
            {
                item = reg.itemDeclaration;
                return true;
            }

            item = default;
            return false;
        }

        public bool TryGetReferenceItemById(ItemDeclarationId id, [MaybeNullWhen(false)] out IReferenceItemDeclaration<TItemId> item)
        {
            return referenceItemsById.TryGetValue(id, out item);
        }

        public IBulkItemIdMapping BulkItemMapping => this;

        public IEnumerable<int> Ids => bulkItemReverseIndex.Keys;

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