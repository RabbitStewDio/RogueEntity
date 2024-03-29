﻿using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Base;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemResolver<TItemId> : IItemResolver<TItemId>
        where TItemId : struct, IEntityKey
    {
        readonly ItemRegistry<TItemId> registry;
        readonly EntityRegistry<TItemId> entityRegistry;
        readonly IBulkDataStorageMetaData<TItemId> entityMetaData;

        public ItemResolver(ItemRegistry<TItemId> registry, 
                            EntityRegistry<TItemId> entityRegistry)
        {
            this.registry = registry;
            this.entityRegistry = entityRegistry;
            this.entityMetaData = registry.EntityMetaData;
            this.QueryProvider = new ReferenceEntityQueryProvider<TItemId>(this);
        }

        public IItemRegistry ItemRegistry => registry;

        public IBulkDataStorageMetaData<TItemId> EntityMetaData => entityMetaData;

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

        public bool TryResolve(in TItemId itemRef, [MaybeNullWhen(false)] out IItemDeclaration item)
        {
            if (itemRef.IsEmpty)
            {
                item = default;
                return false;
            }
            
            if (entityMetaData.IsReferenceEntity(itemRef))
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
        public bool TryQueryTrait<TItemTrait>(TItemId itemRef, [MaybeNullWhen(false)] out TItemTrait data)
            where TItemTrait : IItemTrait
        {
            if (TryResolve(itemRef, out var itemDeclaration))
            {
                return itemDeclaration.TryQuery(out data);
            }

            data = default;
            return false;
        }

        public bool TryQueryData<TData>(TItemId itemRef, [MaybeNullWhen(false)] out TData data)
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
            if (!entityMetaData.IsReferenceEntity(item))
            {
                return;
            }

            if (entityRegistry.IsValid(item))
            {
                entityRegistry.AssignOrReplace<DestroyedMarker>(item);
                entityRegistry.Destroy(item);
            }
        }

        public void Destroy(in TItemId item)
        {
            if (!entityMetaData.IsReferenceEntity(item))
            {
                return;
            }

            if (entityRegistry.IsValid(item))
            {
                entityRegistry.AssignOrReplace<DestroyedMarker>(item);
            }
        }

        public void DestroyNext(in TItemId item)
        {
            if (!entityMetaData.IsReferenceEntity(item))
            {
                return;
            }

            if (entityRegistry.IsValid(item))
            {
                entityRegistry.AssignOrReplace<CascadingDestroyedMarker>(item);
            }
        }

        public bool IsDestroyed(in TItemId item)
        {
            if (item.IsEmpty)
            {
                return true;
            }
            
            if (!entityMetaData.IsReferenceEntity(item))
            {
                return false;
            }

            if (!entityRegistry.IsValid(item))
            {
                return true;
            }

            return entityRegistry.HasComponent<DestroyedMarker>(item) ||
                   entityRegistry.HasComponent<CascadingDestroyedMarker>(item);

        }

        public void Apply(TItemId item)
        {
            if (!entityMetaData.IsReferenceEntity(item))
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

        public IReferenceEntityQueryProvider<TItemId> QueryProvider { get; }


        class ReferenceEntityQueryProvider<TEntityId> : IReferenceEntityQueryProvider<TEntityId>
            where TEntityId : struct, IEntityKey
        {
            readonly ItemResolver<TEntityId> resolver;

            public ReferenceEntityQueryProvider(ItemResolver<TEntityId> resolver)
            {
                this.resolver = resolver;
            }

            public IEnumerable<TEntityId> QueryById(ItemDeclarationId id)
            {
                foreach (var e in resolver.entityRegistry)
                {
                    if (!resolver.entityRegistry.GetComponent(e, out ItemDeclarationHolder<TEntityId> c))
                    {
                        continue;
                    }

                    if (c.ItemId == id)
                    {
                        yield return e;
                    }
                }
            }

            public IEnumerable<(TEntityId, TEntityTraitA)> QueryByTrait<TEntityTraitA>()
            {
                foreach (var e in resolver.entityRegistry.View<TEntityTraitA>())
                {
                    if (!resolver.entityRegistry.GetComponent<ItemDeclarationHolder<TEntityId>>(e, out _) ||
                        !resolver.entityRegistry.GetComponent<TEntityTraitA>(e, out var ca))
                    {
                        continue;
                    }

                    yield return (e, ca);
                }
            }

            public IEnumerable<(TEntityId, TEntityTraitA, TEntityTraitB)> QueryByTrait<TEntityTraitA, TEntityTraitB>()
            {
                foreach (var e in resolver.entityRegistry.View<TEntityTraitA>())
                {
                    if (!resolver.entityRegistry.GetComponent<ItemDeclarationHolder<TEntityId>>(e, out  _) ||
                        !resolver.entityRegistry.GetComponent<TEntityTraitA>(e, out var ca) ||
                        !resolver.entityRegistry.GetComponent<TEntityTraitB>(e, out var cb))
                    {
                        continue;
                    }

                    yield return (e, ca, cb);
                }
                
            }
        }

    }
}