using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.UseEffects
{
    public class UsableItemTrait<TGameContext, TActorId, TItemId>: StatelessItemComponentTraitBase<TGameContext, TItemId, IUsableItemEffect<TGameContext, TActorId, TItemId>>
        where TItemId : IEntityKey
        where TActorId : IEntityKey
    {
        readonly IUsableItemEffect<TGameContext, TActorId, TItemId> effect;

        public UsableItemTrait(IUsableItemEffect<TGameContext, TActorId, TItemId> effect) : base("Core.Item.UseEffect", 300)
        {
            this.effect = effect;
        }

        protected override IUsableItemEffect<TGameContext, TActorId, TItemId> GetData(TGameContext context, TItemId k)
        {
            return effect;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            throw new System.NotImplementedException();
        }
    }
}