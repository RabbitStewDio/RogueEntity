using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Inventory
{
    public class ListInventoryTrait<TGameContext, TOwnerId, TItemId> : IReferenceItemTrait<TGameContext, TOwnerId>,
                                                                       IItemComponentTrait<TGameContext, TOwnerId, IInventory<TGameContext, TItemId>>,
                                                                       IItemComponentTrait<TGameContext, TOwnerId, IInventoryView<TItemId>>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TOwnerId : IBulkDataStorageKey<TOwnerId>
    {
        readonly IItemResolver<TGameContext, TOwnerId> ownerResolver;
        readonly IItemResolver<TGameContext, TItemId> itemResolver;
        readonly Weight defaultCarryWeight;

        public string Id => "Core.Inventory.ListInventory";
        public int Priority => 100;

        protected ListInventoryTrait(IItemResolver<TGameContext, TOwnerId> ownerResolver,
                                     IItemResolver<TGameContext, TItemId> itemResolver,
                                     Weight defaultCarryWeight = default)
        {
            this.ownerResolver = ownerResolver;
            this.itemResolver = itemResolver;
            this.defaultCarryWeight = defaultCarryWeight;
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

        protected bool TryQueryData(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, out ListInventoryData<TOwnerId, TItemId> t)
        {
            if (!k.IsReference)
            {
                t = default;
                return false;
            }

            if (!v.IsValid(k) ||
                !v.GetComponent(k, out t))
            {
                t = default;
                return false;
            }

            return true;
        }

        protected virtual bool ValidateData(IEntityViewControl<TOwnerId> entityViewControl, TGameContext context,
                                            in TOwnerId itemReference, in ListInventoryData<TOwnerId, TItemId> data)
        {
            return true;
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
            if (v.IsValid(k))
            {
                v.RemoveComponent<ListInventoryData<TOwnerId, TItemId>>(k);
                changedK = k;
                return true;
            }

            changedK = k;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, out IInventoryView<TItemId> t)
        {
            if (TryQueryData(v, context, k, out ListInventoryData<TOwnerId, TItemId> data))
            {
                t = new ListInventory<TGameContext, TOwnerId, TItemId>(ownerResolver, itemResolver, data);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryQuery(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, out IInventory<TGameContext, TItemId> t)
        {
            if (TryQueryData(v, context, k, out ListInventoryData<TOwnerId, TItemId> data))
            {
                t = new ListInventory<TGameContext, TOwnerId, TItemId>(ownerResolver, itemResolver, data);
                return true;
            }

            t = default;
            return false;
        }

        public bool TryUpdate(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, in IInventoryView<TItemId> t, out TOwnerId changedK)
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
    }
}