using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Meta.ItemTraits
{
    public class ItemWeightTrait<TGameContext, TItemId> : SimpleItemComponentTraitBase<TGameContext, TItemId, WeightView> 
        where TItemId : IBulkDataStorageKey<TItemId> 
        where TGameContext : IItemContext<TGameContext, TItemId>
    {
        public ItemWeightTrait() : base("Core.Actor.WeightView", 50000)
        {
        }

        protected override WeightView CreateInitialValue(TGameContext c, TItemId reference)
        {
            return new WeightView();
        }

        public override void Initialize(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (!TryQueryBulkData(v, context, k, out var initialValue))
            {
                return;
            }

            v.AssignComponent(k, initialValue);
        }

        public override void Apply(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, IItemDeclaration item)
        {
            if (TryQueryBulkData(v, context, k, out var initialValue))
            {
                v.AssignOrReplace(k, initialValue);
            }
        }

        protected override bool TryQueryBulkData(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out WeightView t)
        {
            var stackCount = context.QueryStackSize(k);
            var itemWeight = context.QueryBaseWeight(k);
            t = new WeightView(itemWeight, Weight.Empty, Weight.Empty, itemWeight * stackCount.Count);
            return true;
        }
    }
}