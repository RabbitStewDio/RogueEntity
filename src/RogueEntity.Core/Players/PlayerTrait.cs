using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Players
{
    public class PlayerTrait<TActorId> : SimpleReferenceItemComponentTraitBase<TActorId, PlayerTag>
        where TActorId : struct, IEntityKey
    {
        public PlayerTrait() : base("Actor.Generic.Player", 1)
        { }

        protected override Optional<PlayerTag> CreateInitialValue(TActorId reference)
        {
            return Optional.Empty();
        }

        public override void Initialize(IEntityViewControl<TActorId> v, TActorId k, IItemDeclaration item)
        {
            base.Initialize(v, k, item);
            v.AssignComponent<NewPlayerSpawnRequest>(k);
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return PlayerModule.PlayerRole.Instantiate<TActorId>();
        }

        public override IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}
