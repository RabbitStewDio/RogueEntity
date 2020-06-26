using System;
using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public abstract class CommandHandlerBase<TGameContext, TActorId, TCommand> : ICommandHandler<TGameContext, TActorId> 
        where TActorId : IEntityKey
        where TCommand: ICommand
    {
        public void Invoke(IEntityViewControl<TActorId> v, TGameContext context, TActorId entity, ICommand command)
        {
            if (!(command is TCommand m))
            {
                throw new InvalidCastException("Given command is not a valid command for this handler");
            }

            if (!v.GetComponent(entity, out IReferenceItemDeclaration<TGameContext, TActorId> entityInfo))
            {
                throw new InvalidOperationException("Not an actor entity.");
            }

            Invoke(context, entity, entityInfo, m);
        }

        public bool CanHandle(ICommand commandId)
        {
            return commandId is TCommand;
        }

        protected abstract void Invoke(TGameContext context, 
                                       TActorId actorKey,
                                       IReferenceItemDeclaration<TGameContext, TActorId> actorData, 
                                       TCommand command);

    }
}