using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Infrastructure.Commands;

namespace RogueEntity.Core.Infrastructure.Actions.Idle
{
    public class IdleCommandHandler<TActorId> : CommandHandlerBase<TActorId, IdleCommand>
        where TActorId : IEntityKey
    {
        public string ActionId => IdleCommand.ActionId;

        protected override bool Invoke(TActorId actorKey,
                                       IReferenceItemDeclaration<TActorId> actorData,
                                       IdleCommand command)
        {
            // todo
            // context.RunNext(actorKey, new IdleAction<TActorId>(command.Turns));
            return true;
        }
    }
}
