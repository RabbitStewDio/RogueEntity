using EnTTSharp.Entities;
using RogueEntity.Core.Inputs.Commands;

namespace RogueEntity.Core.Infrastructure.Commands
{
    /// <summary>
    ///   Takes a command and translates it into an entity-system state that can be used
    ///   by other systems. Most commonly this schedules an action for a player character
    ///   to perform.
    /// </summary>
    public interface ICommandHandler<TActorId> where TActorId : IEntityKey
    {
        bool Invoke(IEntityViewControl<TActorId> v,
                    TActorId entity,
                    ICommand command);

        bool CanHandle(ICommand commandId);
    }
}