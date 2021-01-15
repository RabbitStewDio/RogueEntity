using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Traits
{
    public class MovementPointsTrait<TActorId> : SimpleReferenceItemComponentTraitBase<TActorId, MovementPoints>
        where TActorId : IEntityKey
    {
        readonly int initialValue;

        public MovementPointsTrait(int initialValue = 0) : base("Core.Actor.MovementPoints", 100)
        {
            this.initialValue = initialValue;
        }

        protected override MovementPoints CreateInitialValue(TActorId reference)
        {
            return MovementPoints.From(initialValue);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            throw new System.NotImplementedException();
        }
    }
}
