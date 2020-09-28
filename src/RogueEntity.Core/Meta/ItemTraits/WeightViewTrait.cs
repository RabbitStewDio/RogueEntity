using System;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public class WeightViewTrait<TGameContext, TItemId> : StatelessItemComponentTraitBase<TGameContext, TItemId, WeightView>
        where TItemId : IEntityKey
    {
        readonly IItemResolver<TGameContext, TItemId> itemResolver;

        public WeightViewTrait(IItemResolver<TGameContext, TItemId> itemResolver) : base("Core.Common.WeightView", 50000)
        {
            this.itemResolver = itemResolver;
        }

        public override bool TryQuery(IEntityViewControl<TItemId> v, TGameContext context, TItemId k, out WeightView t)
        {
            var stackCount = itemResolver.QueryStackSize(k, context);
            var itemWeight = itemResolver.QueryBaseWeight(k, context);
            var inventoryWeight = Weight.Empty;
            
            if (itemResolver.TryResolve(k, out var declaration))
            {
                var traits = declaration.QueryAll<IItemComponentInformationTrait<TGameContext, TItemId, InventoryWeight>>();
                foreach (var tr in traits)
                {
                    if (tr.TryQuery(v, context, k, out var iv))
                    {
                        inventoryWeight += iv.Weight;
                    }
                }
            }
           
            t =  new WeightView(itemWeight, inventoryWeight, itemWeight * stackCount.Count);
            return true;
        }

        protected override WeightView GetData(TGameContext context, TItemId k)
        {
            throw new InvalidOperationException();
        }
    }
}