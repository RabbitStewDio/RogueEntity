using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public enum EvictionBehaviour
    {
        Destroy,
        RemoveAndPreserve
    }
    
    public class EvictionBehaviourTrait<TActorId>: StatelessItemComponentTraitBase<TActorId, EvictionBehaviour>
        where TActorId : struct, IEntityKey
    {
        readonly EvictionBehaviour behaviour;
        
        public EvictionBehaviourTrait(EvictionBehaviour behaviour, int priority = 100) : base("Trait.EvictionBehaviour", priority)
        {
            this.behaviour = behaviour;
        }

        protected override EvictionBehaviour GetData(TActorId k)
        {
            return behaviour;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            return Enumerable.Empty<EntityRoleInstance>();
        }
    }
}
