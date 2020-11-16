using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Actions.Schedule;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Actions.Idle
{
    public class IdleCommandHandler<TGameContext, TActorId> : CommandHandlerBase<TGameContext, TActorId, IdleCommand> 
        where TActorId : IEntityKey
        where TGameContext: IItemContext<TGameContext, TActorId>
    {
        public string ActionId => IdleCommand.ActionId;

        protected override bool Invoke(TGameContext context, TActorId actorKey, 
                                       IReferenceItemDeclaration<TGameContext, TActorId> actorData, IdleCommand command)
        {
            context.RunNext(actorKey, new IdleAction<TGameContext, TActorId>(command.Turns));
            return true;
        }
    }
}