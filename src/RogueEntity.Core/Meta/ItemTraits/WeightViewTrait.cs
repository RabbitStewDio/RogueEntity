using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public class WeightViewTrait< TItemId> : StatelessItemComponentTraitBase< TItemId, WeightView>
        where TItemId : struct, IEntityKey
    {
        readonly IItemResolver< TItemId> itemResolver;

        public WeightViewTrait(IItemResolver< TItemId> itemResolver) : base("Core.Common.WeightView", 50000)
        {
            this.itemResolver = itemResolver;
        }

        public override bool TryQuery(IEntityViewControl<TItemId> v, TItemId k, out WeightView t)
        {
            var stackCount = itemResolver.QueryStackSize(k);
            var itemWeight = itemResolver.QueryBaseWeight(k);
            var inventoryWeight = Weight.Empty;
            
            if (itemResolver.TryResolve(k, out var declaration))
            {
                var traits = declaration.QueryAll<IItemComponentInformationTrait< TItemId, InventoryWeight>>();
                foreach (var tr in traits)
                {
                    if (tr.TryQuery(v, k, out var iv))
                    {
                        inventoryWeight += iv.Weight;
                    }
                }
            }
           
            t =  new WeightView(itemWeight, inventoryWeight, itemWeight * stackCount.Count);
            return true;
        }

        protected override WeightView GetData(TItemId k)
        {
            throw new InvalidOperationException("This should never be callable.");
        }
        
        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.ItemRole.Instantiate<TItemId>();
        }
    }
}