﻿using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Meta.ItemTraits
{
    public class ItemWeightTrait<TGameContext, TItemId> : StatelessItemComponentTraitBase<TGameContext, TItemId, WeightView> 
        where TItemId : IBulkDataStorageKey<TItemId> 
    {
        readonly IItemResolver<TGameContext, TItemId> itemResolver;

        public ItemWeightTrait(IItemResolver<TGameContext, TItemId> itemResolver) : base("Core.Actor.WeightView", 50000)
        {
            this.itemResolver = itemResolver;
        }

        protected override WeightView GetData(TGameContext context, TItemId k)
        {
            var stackCount = itemResolver.QueryStackSize(k, context);
            var itemWeight = itemResolver.QueryBaseWeight(k, context);
            return new WeightView(itemWeight, Weight.Empty, Weight.Empty, itemWeight * stackCount.Count);
        }
    }
}