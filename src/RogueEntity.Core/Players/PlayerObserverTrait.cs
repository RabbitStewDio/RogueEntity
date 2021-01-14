using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Players
{
    public class PlayerObserverTrait<TGameContext, TActorId> : SimpleReferenceItemComponentTraitBase<TGameContext, TActorId, PlayerTag>
        where TActorId : IEntityKey
    {
        public PlayerObserverTrait() : base("Actor.Generic.PlayerObserver", 1)
        { }

        public override void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            // Intentionally empty. To be filled via the service implementation and the store-data methods.
        }

        protected override PlayerTag CreateInitialValue(TGameContext c, TActorId reference)
        {
            throw new System.NotImplementedException();
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
