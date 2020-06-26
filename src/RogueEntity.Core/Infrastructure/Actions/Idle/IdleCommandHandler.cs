using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Idle
{
    public class IdleCommandHandler<TGameContext, TActorId> : CommandHandlerBase<TGameContext, TActorId, IdleCommand> 
        where TActorId : IEntityKey
        where TGameContext: IItemContext<TGameContext, TActorId>
    {
        public string ActionId => IdleCommand.ActionId;

        protected override void Invoke(TGameContext context, TActorId actorKey, 
                                       IReferenceItemDeclaration<TGameContext, TActorId> actorData, IdleCommand command)
        {
            context.RunNext(actorKey, new IdleAction<TGameContext, TActorId>(command.Turns));
        }
    }
}