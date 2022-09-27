using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public sealed class ItemChargeTrait<TItemId> : SimpleReferenceItemComponentTraitBase<TItemId, ItemCharge>
        where TItemId : struct, IEntityKey
    {
        readonly ItemCharge initialCharge;

        public ItemChargeTrait(ushort charge, ushort maxCharge) : base("Core.Item.Charge", 100)
        {
            initialCharge = new ItemCharge(charge, maxCharge);
        }

        protected override Optional<ItemCharge> CreateInitialValue(TItemId reference)
        {
            return initialCharge;
        }

        protected override bool ValidateData(IEntityViewControl<TItemId> v,
                                             in TItemId itemReference,
                                             in ItemCharge data)
        {
            return data.Count < initialCharge.MaximumCharge;
        }

        protected override bool TryRemoveComponentData(IEntityViewControl<TItemId> v, TItemId k, out TItemId changedItem)
        {
            if (TryQuery(v, k, out var existingData))
            {
                return TryUpdate(v, k, existingData.WithCount(0), out changedItem);
            }

            changedItem = k;
            return false;
        }

        public override void Apply(IEntityViewControl<TItemId> v, TItemId k, IItemDeclaration item)
        { }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.ItemRole.Instantiate<TItemId>();
        }
    }

    public sealed class ItemChargeBulkTrait<TItemId> : SimpleBulkItemComponentTraitBase<TItemId, ItemCharge>,
                                                       IBulkDataTrait<TItemId>
        where TItemId : struct, IBulkDataStorageKey<TItemId>
    {
        readonly ItemCharge initialCharge;

        public ItemChargeBulkTrait(ushort charge, ushort maxCharge) : base("Core.Item.Charge", 100)
        {
            initialCharge = new ItemCharge(charge, maxCharge);
        }

        protected override ItemCharge CreateInitialValue(TItemId reference)
        {
            return initialCharge;
        }

        protected override bool TryQueryBulkData(IEntityViewControl<TItemId> v, TItemId k, out ItemCharge t)
        {
            t = initialCharge.WithCount(k.Data);
            return true;
        }

        protected override bool ValidateData(IEntityViewControl<TItemId> v,
                                             in TItemId itemReference,
                                             in ItemCharge data)
        {
            return data.Count < initialCharge.MaximumCharge;
        }

        protected override bool TryUpdateBulkData(TItemId k, in ItemCharge data, out TItemId changedK)
        {
            if (data.Count < initialCharge.MaximumCharge)
            {
                var durability = initialCharge.WithCount(data.Count);
                changedK = k.WithData(durability.Count);
                return true;
            }

            changedK = k;
            return false;
        }

        public override bool TryRemove(IEntityViewControl<TItemId> v, TItemId k, out TItemId changedItem)
        {
            if (TryQuery(v, k, out var existingData))
            {
                return TryUpdate(v, k, existingData.WithCount(0), out changedItem);
            }

            changedItem = k;
            return false;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.ItemRole.Instantiate<TItemId>();
        }
    }
}
