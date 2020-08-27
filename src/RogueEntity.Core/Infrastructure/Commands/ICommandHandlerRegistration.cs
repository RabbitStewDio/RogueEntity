using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public interface ICommandHandlerRegistration<TGameContext, TActorId> 
        where TActorId : IEntityKey
    {
        void Register(ICommandHandler<TGameContext, TActorId> p);
        bool TryGetProcessor(ICommand commandId, out ICommandHandler<TGameContext, TActorId> p);

    }
}