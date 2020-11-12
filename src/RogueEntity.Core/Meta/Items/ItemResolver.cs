using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Base;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemResolver<TGameContext, TItemId> : IItemResolver<TGameContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly ItemRegistry<TGameContext, TItemId> registry;
        readonly EntityRegistry<TItemId> entityRegistry;

        public ItemResolver(ItemRegistry<TGameContext, TItemId> registry, 
                            EntityRegistry<TItemId> entityRegistry)
        {
            this.registry = registry;
            this.entityRegistry = entityRegistry;
        }

        public IItemRegistry ItemRegistry => registry;

        public TItemId Instantiate(TGameContext context, IItemDeclaration item)
        {
            if (item is IBulkItemDeclaration<TGameContext, TItemId> bulkItem)
            {
                return InstantiateBulkItem(context, bulkItem);
            }

            return InstantiateReferenceItem(context, (IReferenceItemDeclaration<TGameContext, TItemId>)item);
        }

        TItemId InstantiateReferenceItem(TGameContext context, IReferenceItemDeclaration<TGameContext, TItemId> itemDeclaration)
        {
            var entity = entityRegistry.Create();
            entityRegistry.AssignComponent(entity, new ItemDeclarationHolder<TGameContext, TItemId>(itemDeclaration));
            itemDeclaration.Initialize(entityRegistry, context, entity);
            return entity;
        }

        TItemId InstantiateBulkItem(TGameContext context,
                                    IBulkItemDeclaration<TGameContext, TItemId> item)
        {
            var id = registry.GenerateBulkItemId(item);
            return item.Initialize(context, id);
        }

        public bool TryResolve(in TItemId itemRef, out IItemDeclaration item)
        {
            if (itemRef.IsReference)
            {
                if (entityRegistry.IsValid(itemRef))
                {
                    if (entityRegistry.GetComponent(itemRef, out ItemDeclarationHolder<TGameContext, TItemId> ri))
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

        public bool TryQueryData<TData>(TItemId itemRef, TGameContext context, out TData data)
        {
            if (TryQueryTrait<IItemComponentInformationTrait<TGameContext, TItemId, TData>>(itemRef, out var trait))
            {
                return trait.TryQuery(entityRegistry, context, itemRef, out data);
            }

            data = default;
            return false;
        }

        public bool TryUpdateData<TData>(TItemId itemRef,
                                         TGameContext context,
                                         in TData data,
                                         out TItemId changedItem)
        {
            if (TryQueryTrait<IItemComponentTrait<TGameContext, TItemId, TData>>(itemRef, out var trait))
            {
                return trait.TryUpdate(entityRegistry, context, itemRef, in data, out changedItem);
            }

            changedItem = itemRef;
            return false;
        }

        public bool TryRemoveData<TData>(TItemId itemRef, TGameContext context, out TItemId changedItem)
        {
            if (TryQueryTrait<IItemComponentTrait<TGameContext, TItemId, TData>>(itemRef, out var trait))
            {
                return trait.TryRemove(entityRegistry, context, itemRef, out changedItem);
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

        public void Apply(TItemId item, TGameContext context)
        {
            if (!item.IsReference)
            {
                return;
            }

            if (entityRegistry.IsValid(item))
            {
                if (entityRegistry.GetComponent(item, out ItemDeclarationHolder<TGameContext, TItemId> ri))
                {
                    ri.ItemDeclaration.Apply(entityRegistry, context, item);
                }
            }
        }
    }
}