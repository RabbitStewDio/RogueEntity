using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Meta.ItemTraits
{
    public class ItemChargeTrait<TGameContext, TItemId> : SimpleItemComponentTraitBase<TGameContext, TItemId, ItemCharge>,
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

        protected override bool ValidateData(IEntityViewControl<TItemId> entityViewControl, TGameContext context, in TItemId itemReference,
                                             in ItemCharge data)
        {
            return data.Count < initialCharge.MaximumCharge;
        }

        protected override bool TryUpdateBulkData(TItemId k, in ItemCharge data, out TItemId changedK)
        {
            var durability = initialCharge.WithCount(data.Count);
            changedK = k.WithData(durability.Count);
            return true;
        }
    }
}