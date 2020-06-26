using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Actions;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public class CommandReceiverTrait<TGameContext, TActorId> : IReferenceItemTrait<TGameContext, TActorId> 
        where TActorId : IEntityKey
    {
        public CommandReceiverTrait()
        {
            Id = "Actor.Generic.CommandReceiver";
            Priority = 1;
        }

        public string Id { get; }
        public int Priority { get; }

        public void Initialize(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
            v.AssignComponent<IdleMarker>(k);
            v.AssignComponent<CommandQueueComponent>(k);
        }

        public void Apply(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, IItemDeclaration item)
        {
        }
    }
}