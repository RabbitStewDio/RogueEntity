using EnTTSharp.Entities;
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

        public string Id { get; }
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
    }
}