using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Inventory
{
    public class ListInventoryTrait<TGameContext, TOwnerId, TItemId> : IReferenceItemTrait<TGameContext, TOwnerId>,
                                                                       IItemComponentTrait<TGameContext, TOwnerId, IInventory<TGameContext, TItemId>>,
                                                                       IItemComponentTrait<TGameContext, TOwnerId, IContainerView<TItemId>>,
                                                                       IItemComponentInformationTrait<TGameContext, TOwnerId, InventoryWeight>
        where TOwnerId : IEntityKey
        where TItemId : IEntityKey
    {
        static readonly EqualityComparer<TOwnerId> Comparer = EqualityComparer<TOwnerId>.Default;
        readonly IBulkDataStorageMetaData<TItemId> itemIdMetaData;
        readonly IItemResolver<TGameContext, TItemId> itemResolver;
        readonly Weight defaultCarryWeight;

        public ItemTraitId Id => "Core.Inventory.ListInventory";
        public int Priority => 100;

        public ListInventoryTrait([NotNull] IBulkDataStorageMetaData<TItemId> itemIdMetaData,
                                  [NotNull] IItemResolver<TGameContext, TItemId> itemResolver,
                                  Weight defaultCarryWeight = default)
        {
            this.itemIdMetaData = itemIdMetaData ?? throw new ArgumentNullException(nameof(itemIdMetaData));
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.defaultCarryWeight = defaultCarryWeight;
        }

        public IReferenceItemTrait<TGameContext, TOwnerId> CreateInstance()
        {
            return this;
        }

        protected virtual ListInventoryData<TOwnerId, TItemId> CreateInitialValue(TGameContext c, TOwnerId actor)
        {
            return new ListInventoryData<TOwnerId, TItemId>(actor, defaultCarryWeight);
        }

        public virtual void Initialize(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, IItemDeclaration item)
        {
            v.AssignOrReplace(k, CreateInitialValue(context, k));
        }

        public virtual void Apply(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, IItemDeclaration item)
        {
        }

        protected bool TryQueryData(IEntityViewControl<TOwnerId> v, TOwnerId k, out ListInventoryData<TOwnerId, TItemId> t)
        {
            if (!v.IsValid(k) ||
                !v.GetComponent(k, out t))
            {
                t = default;
                return false;
            }

            return true;
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        protected virtual bool ValidateData(IEntityViewControl<TOwnerId> entityViewControl,
                                            TGameContext context,
                                            in TOwnerId itemReference,
                                            in ListInventoryData<TOwnerId, TItemId> data)
        {
            return Comparer.Equals(data.OwnerData, itemReference);
        }

        protected bool TryUpdateData(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, in ListInventoryData<TOwnerId, TItemId> t, out TOwnerId changedK)
        {
            if (!ValidateData(v, context, k, in t))
            {
                changedK = k;
                return false;
            }

            if (v.IsValid(k))
            {
                v.AssignOrReplace(k, in t);
                changedK = k;
                return true;
            }

            changedK = k;
            return false;
        }

        public bool TryRemove(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, out TOwnerId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, out InventoryWeight t)
        {
            if (TryQueryData(v, k, out ListInventoryData<TOwnerId, TItemId> inventory))
            {
                t= new InventoryWeight(inventory.TotalWeight);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, out IContainerView<TItemId> t)
        {
            if (TryQueryData(v, k, out ListInventoryData<TOwnerId, TItemId> data))
            {
                t = new ListInventory<TGameContext, TOwnerId, TItemId>(itemIdMetaData, itemResolver, data);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, out IInventory<TGameContext, TItemId> t)
        {
            if (TryQueryData(v, k, out ListInventoryData<TOwnerId, TItemId> data))
            {
                t = new ListInventory<TGameContext, TOwnerId, TItemId>(itemIdMetaData, itemResolver, data);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, in IContainerView<TItemId> t, out TOwnerId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, in IInventory<TGameContext, TItemId> t, out TOwnerId changedK)
        {
            if (!(t is ListInventory<TGameContext, TOwnerId, TItemId> inv))
            {
                changedK = k;
                return false;
            }

            if (TryUpdateData(v, context, k, inv.Data, out changedK))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return InventoryModule.ContainerRole.Instantiate<TOwnerId>();
            yield return InventoryModule.ContainedItemRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            yield return InventoryModule.ContainsRelation.Instantiate<TOwnerId, TItemId>();
        }
    }
}