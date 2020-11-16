using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public class ActionPointsTrait<TGameContext, TActorId> : SimpleReferenceItemComponentTraitBase<TGameContext, TActorId, ActionPoints>
        where TActorId : IEntityKey
    {
        readonly int initialValue;

        public ActionPointsTrait(int initialValue = 0) : base("Core.Actor.ActionPoints", 100)
        {
            this.initialValue = initialValue;
        }

        protected override ActionPoints CreateInitialValue(TGameContext c, TActorId actor)
        {
            return ActionPoints.From(initialValue);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            throw new System.NotImplementedException();
        }
    }
}