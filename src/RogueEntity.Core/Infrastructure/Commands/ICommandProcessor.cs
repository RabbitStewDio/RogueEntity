using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Actions;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public interface ICommandProcessor<TGameContext, TActorId> 
        where TActorId : IEntityKey
    {
        void ProcessActions(IEntityViewControl<TActorId> v, 
                            TGameContext context,
                            TActorId entity,
                            in CommandQueueComponent commandQueue,
                            in IdleMarker idleConstraint);
    }
}