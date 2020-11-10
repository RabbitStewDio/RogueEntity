using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public sealed class ItemChargeTrait<TGameContext, TItemId> : SimpleItemComponentTraitBase<TGameContext, TItemId, ItemCharge>,
                                                          IBulkDataTrait<TGameContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly ItemCharge initialCharge;

        public ItemChargeTrait(ushort charge, ushort maxCharge) : base("Core.Item.Charge", 100)
        {
            initialCharge = new ItemCharge(charge, maxCharge);
        }

        protected override ItemCharge CreateInitialValue(TGameContext c, TItemId reference)
        {
            return initialCharge;
        }

        protected override bool TryQueryBulkData(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out ItemCharge t)
        {
            t = initialCharge.WithCount(k.Data);
            return true;
        }

        protected override bool ValidateData(IEntityViewControl<TItemId> v, TGameContext context, in TItemId itemReference,
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

        public override bool TryRemove(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out TItemId changedItem)
        {
            if (TryQuery(v, context, k, out var existingData))
            {
                return TryUpdate(v, context, k, existingData.WithCount(0), out changedItem);
            }

            changedItem = k;
            return false;
        }

        public override void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.ItemRole.Instantiate<TItemId>();
        }
    }
}