using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Base;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemResolver<TItemId> : IItemResolver<TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly ItemRegistry<TItemId> registry;
        readonly EntityRegistry<TItemId> entityRegistry;

        public ItemResolver(ItemRegistry<TItemId> registry, 
                            EntityRegistry<TItemId> entityRegistry)
        {
            this.registry = registry;
            this.entityRegistry = entityRegistry;
        }

        public IItemRegistry ItemRegistry => registry;

        public TItemId Instantiate(IItemDeclaration item)
        {
            if (item is IBulkItemDeclaration<TItemId> bulkItem)
            {
                return InstantiateBulkItem(bulkItem);
            }

            return InstantiateReferenceItem((IReferenceItemDeclaration<TItemId>)item);
        }

        TItemId InstantiateReferenceItem(IReferenceItemDeclaration<TItemId> itemDeclaration)
        {
            var entity = entityRegistry.Create();
            entityRegistry.AssignComponent(entity, new ItemDeclarationHolder<TItemId>(itemDeclaration));
            itemDeclaration.Initialize(entityRegistry, entity);
            return entity;
        }

        TItemId InstantiateBulkItem(IBulkItemDeclaration<TItemId> item)
        {
            var id = registry.GenerateBulkItemId(item);
            return item.Initialize(id);
        }

        public bool TryResolve(in TItemId itemRef, out IItemDeclaration item)
        {
            if (itemRef.IsReference)
            {
                if (entityRegistry.IsValid(itemRef))
                {
                    if (entityRegistry.GetComponent(itemRef, out ItemDeclarationHolder<TItemId> ri))
                    {
                        item = ri.ItemDeclaration;
                        return true;
                    }

                }

                item = default;
                return false;
            }

            if (registry.TryResolveBulkItem(itemRef, out var bulkItem))
            {
                item = bulkItem;
                return true;
            }

            item = default;
            return false;
        }

        /// <summary>
        ///  Attempts to query additional data for referenced actors. Simple
        ///  actors cannot be queried, they exist only implicitly within the map
        ///  data.
        /// </summary>
        /// <typeparam name="TItemTrait"></typeparam>
        /// <param name="itemRef"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool TryQueryTrait<TItemTrait>(TItemId itemRef, out TItemTrait data)
            where TItemTrait : IItemTrait
        {
            if (TryResolve(itemRef, out var itemDeclaration))
            {
                return itemDeclaration.TryQuery(out data);
            }

            data = default;
            return false;
        }

        public bool TryQueryData<TData>(TItemId itemRef, out TData data)
        {
            if (TryQueryTrait<IItemComponentInformationTrait<TItemId, TData>>(itemRef, out var trait))
            {
                return trait.TryQuery(entityRegistry, itemRef, out data);
            }

            data = default;
            return false;
        }

        public bool TryUpdateData<TData>(TItemId itemRef,
                                         in TData data,
                                         out TItemId changedItem)
        {
            if (TryQueryTrait<IItemComponentTrait<TItemId, TData>>(itemRef, out var trait))
            {
                return trait.TryUpdate(entityRegistry, itemRef, in data, out changedItem);
            }

            changedItem = itemRef;
            return false;
        }

        public bool TryRemoveData<TData>(TItemId itemRef,  out TItemId changedItem)
        {
            if (TryQueryTrait<IItemComponentTrait<TItemId, TData>>(itemRef, out var trait))
            {
                return trait.TryRemove(entityRegistry, itemRef, out changedItem);
            }

            changedItem = itemRef;
            return false;
        }

        public void DiscardUnusedItem(in TItemId item)
        {
            if (!item.IsReference)
            {
                return;
            }

            if (entityRegistry.IsValid(item))
            {
                entityRegistry.AssignOrReplace<DestroyedMarker>(item);
                entityRegistry.Destroy(item);
            }
        }

        public TItemId Destroy(in TItemId item)
        {
            if (!item.IsReference)
            {
                return default;
            }

            if (entityRegistry.IsValid(item))
            {
                entityRegistry.AssignOrReplace<DestroyedMarker>(item);
                return default;
            }

            return item;
        }

        public TItemId DestroyNext(in TItemId item)
        {
            if (!item.IsReference)
            {
                return default;
            }

            if (entityRegistry.IsValid(item))
            {
                entityRegistry.AssignOrReplace<CascadingDestroyedMarker>(item);
                return default;
            }

            return item;
        }

        public void Apply(TItemId item)
        {
            if (!item.IsReference)
            {
                return;
            }

            if (entityRegistry.IsValid(item))
            {
                if (entityRegistry.GetComponent(item, out ItemDeclarationHolder<TItemId> ri))
                {
                    ri.ItemDeclaration.Apply(entityRegistry, item);
                }
            }
        }
    }
}