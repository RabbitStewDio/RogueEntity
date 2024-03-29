using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    /// <summary>
    ///    Defines the base weight of an item. This trait provides a static value under the assumption
    ///    that items do not change their inherent weight. Chests and other container items can still
    ///    gain weight if needed. 
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public class WeightTrait< TItemId> : StatelessItemComponentTraitBase< TItemId, Weight>
        where TItemId : struct, IEntityKey
    {
        readonly Weight baseWeight;

        public WeightTrait(Weight baseWeight) : base("Core.Common.BaseWeight", 100)
        {
            this.baseWeight = baseWeight;
        }

        protected override Weight GetData(TItemId k)
        {
            return baseWeight;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.ItemRole.Instantiate<TItemId>();
        }
        
    }
}