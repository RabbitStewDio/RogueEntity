using System;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Inputs.Commands;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public abstract class CommandHandlerBase<TActorId, TCommand> : ICommandHandler<TActorId>
        where TActorId : IEntityKey
        where TCommand : ICommand
    {
        public bool Invoke(IEntityViewControl<TActorId> v, TActorId entity, ICommand command)
        {
            if (!(command is TCommand m))
            {
                throw new InvalidCastException("Given command is not a valid command for this handler");
            }

            if (!v.GetComponent(entity, out IReferenceItemDeclaration<TActorId> entityInfo))
            {
                throw new InvalidOperationException("Not an actor entity.");
            }

            return Invoke(entity, entityInfo, m);
        }

        public bool CanHandle(ICommand commandId)
        {
            return commandId is TCommand;
        }

        protected abstract bool Invoke(TActorId actorKey,
                                       IReferenceItemDeclaration<TActorId> actorData,
                                       TCommand command);
    }
}
