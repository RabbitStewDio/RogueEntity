using EnTTSharp.Entities;
using RogueEntity.Core.Inputs.Commands;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public interface ICommandHandlerRegistration< TActorId> 
        where TActorId : IEntityKey
    {
        void Register(ICommandHandler< TActorId> p);
        bool TryGetProcessor(ICommand commandId, out ICommandHandler< TActorId> p);

    }
}