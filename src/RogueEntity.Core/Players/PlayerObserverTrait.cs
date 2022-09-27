using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Players
{
    public class PlayerObserverTrait<TActorId> : SimpleReferenceItemComponentTraitBase<TActorId, PlayerObserverTag>
        where TActorId : struct, IEntityKey
    {
        public PlayerObserverTrait() : base("Actor.Generic.PlayerObserver", 1)
        {
        }

        protected override Optional<PlayerObserverTag> CreateInitialValue(TActorId reference)
        {
            return Optional.Empty();
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return PlayerModule.PlayerObserverRole.Instantiate<TActorId>();
        }

        public override IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}
