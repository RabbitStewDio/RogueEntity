using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Base;

namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public class ItemResolver<TContext, TItemId> : IItemResolver<TContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly ItemRegistry<TContext, TItemId> registry;
        readonly EntityRegistry<TItemId> entityRegistry;

        public ItemResolver(ItemRegistry<TContext, TItemId> registry,
                            EntityRegistry<TItemId> entityRegistry)
        {
            this.registry = registry;
            this.entityRegistry = entityRegistry;
        }

        TItemId InstantiateReferenceItem(TContext context, IReferenceItemDeclaration<TContext, TItemId> itemDeclaration)
        {
            var entity = entityRegistry.Create();
            entityRegistry.AssignComponent(entity, in itemDeclaration);
            itemDeclaration.Initialize(entityRegistry, context, entity);
            return entity;
        }

        public TItemId Instantiate(TContext context, IItemDeclaration item)
        {
            if (item is IBulkItemDeclaration<TContext, TItemId> bulkItem)
            {
                return InstantiateBulkItem(bulkItem);
            }

            return InstantiateReferenceItem(context, (IReferenceItemDeclaration<TContext, TItemId>)item);
        }

        TItemId InstantiateBulkItem(IBulkItemDeclaration<TContext, TItemId> item)
        {
            return registry.GenerateBulkItemId(item);
        }

        public bool TryResolve(in TItemId itemRef, out IItemDeclaration item)
        {
            if (entityRegistry.IsValid(itemRef))
            {
                if (entityRegistry.GetComponent(itemRef, out IReferenceItemDeclaration<TContext, TItemId> ri))
                {
                    item = ri;
                    return true;
                }

                // not yet.
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

        public bool TryQueryData<TData>(TItemId itemRef, TContext context, out TData data)
        {
            if (TryQueryTrait<IItemComponentTrait<TContext, TItemId, TData>>(itemRef, out var trait))
            {
                return trait.TryQuery(entityRegistry, context, itemRef, out data);
            }

            data = default;
            return false;
        }

        public bool TryUpdateData<TData>(TItemId itemRef,
                                         TContext context,
                                         in TData data,
                                         out TItemId changedItem)
        {
            if (TryQueryTrait<IItemComponentTrait<TContext, TItemId, TData>>(itemRef, out var trait))
            {
                return trait.TryUpdate(entityRegistry, context, itemRef, in data, out changedItem);
            }

            changedItem = itemRef;
            return false;
        }

        public void DiscardUnusedItem(in TItemId item)
        {
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

        public void Apply(TItemId reference, TContext context)
        {
            if (entityRegistry.IsValid(reference))
            {
                if (entityRegistry.GetComponent(reference, out IReferenceItemDeclaration<TContext, TItemId> ri))
                {
                    ri.Apply(entityRegistry, context, reference);
                }
            }
        }
    }
}