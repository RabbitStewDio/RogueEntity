using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Inventory
{
    public class ListInventoryTrait<TOwnerId, TItemId> : IReferenceItemTrait<TOwnerId>,
                                                         IItemComponentTrait<TOwnerId, IInventory<TItemId>>,
                                                         IItemComponentTrait<TOwnerId, IContainerView<TItemId>>,
                                                         IItemComponentInformationTrait<TOwnerId, InventoryWeight>
        where TOwnerId : struct, IEntityKey
        where TItemId : struct, IEntityKey
    {
        static readonly EqualityComparer<TOwnerId> comparer = EqualityComparer<TOwnerId>.Default;
        readonly IBulkDataStorageMetaData<TItemId> itemIdMetaData;
        readonly IItemResolver<TItemId> itemResolver;
        readonly Weight defaultCarryWeight;

        public ItemTraitId Id => "Core.Inventory.ListInventory";
        public int Priority => 100;

        public ListInventoryTrait(IBulkDataStorageMetaData<TItemId> itemIdMetaData,
                                  IItemResolver<TItemId> itemResolver,
                                  Weight defaultCarryWeight = default)
        {
            this.itemIdMetaData = itemIdMetaData ?? throw new ArgumentNullException(nameof(itemIdMetaData));
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
            this.defaultCarryWeight = defaultCarryWeight;
        }

        public IReferenceItemTrait<TOwnerId> CreateInstance()
        {
            return this;
        }

        protected virtual ListInventoryData<TOwnerId, TItemId> CreateInitialValue(TOwnerId actor)
        {
            return new ListInventoryData<TOwnerId, TItemId>(actor, defaultCarryWeight);
        }

        public virtual void Initialize(IEntityViewControl<TOwnerId> v, TOwnerId k, IItemDeclaration item)
        {
            v.AssignOrReplace(k, CreateInitialValue(k));
        }

        public virtual void Apply(IEntityViewControl<TOwnerId> v, TOwnerId k, IItemDeclaration item)
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
                                            in TOwnerId itemReference,
                                            in ListInventoryData<TOwnerId, TItemId> data)
        {
            return comparer.Equals(data.OwnerData, itemReference);
        }

        protected bool TryUpdateData(IEntityViewControl<TOwnerId> v, TOwnerId k, in ListInventoryData<TOwnerId, TItemId> t, out TOwnerId changedK)
        {
            if (!ValidateData(v, k, in t))
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

        public bool TryRemove(IEntityViewControl<TOwnerId> v, TOwnerId k, out TOwnerId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TOwnerId> v, TOwnerId k, out InventoryWeight t)
        {
            if (TryQueryData(v, k, out ListInventoryData<TOwnerId, TItemId> inventory))
            {
                t = new InventoryWeight(inventory.TotalWeight);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TOwnerId> v, TOwnerId k, [MaybeNullWhen(false)] out IContainerView<TItemId> t)
        {
            if (TryQueryData(v, k, out ListInventoryData<TOwnerId, TItemId> data))
            {
                t = new ListInventory<TOwnerId, TItemId>(itemIdMetaData, itemResolver, data);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TOwnerId> v, TOwnerId k, [MaybeNullWhen(false)] out IInventory<TItemId> t)
        {
            if (TryQueryData(v, k, out ListInventoryData<TOwnerId, TItemId> data))
            {
                t = new ListInventory<TOwnerId, TItemId>(itemIdMetaData, itemResolver, data);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TOwnerId> v, TOwnerId k, in IContainerView<TItemId> t, out TOwnerId changedK)
        {
            changedK = k;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TOwnerId> v, TOwnerId k, in IInventory<TItemId> t, out TOwnerId changedK)
        {
            if (!(t is ListInventory<TOwnerId, TItemId> inv))
            {
                changedK = k;
                return false;
            }

            if (TryUpdateData(v, k, inv.Data, out changedK))
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