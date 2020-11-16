using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Meta.ItemTraits
{
    public class PlayerTrait<TGameContext, TActorId> : IReferenceItemTrait<TGameContext, TActorId> 
        where TActorId : IEntityKey
    {
        public PlayerTrait()
        {
            Id = "Actor.Generic.Player";
            Priority = 1;
        }

        public ItemTraitId Id { get; }
        public int Priority { get; }

        public IReferenceItemTrait<TGameContext, TActorId> CreateInstance()
        {
            return this;
        }

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent<PlayerTag>(k);
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CoreModule.PlayerRole.Instantiate<TActorId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            return Enumerable.Empty<EntityRelationInstance>();
        }
    }
}