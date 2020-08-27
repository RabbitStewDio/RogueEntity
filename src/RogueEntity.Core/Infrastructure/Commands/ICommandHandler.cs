using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Commands
{
    /// <summary>
    ///   Takes a command and translates it into an entity-system state that can be used
    ///   by other systems. Most commonly this schedules an action for a player character
    ///   to perform.
    /// </summary>
    public interface ICommandHandler<in TGameContext, TActorId> where TActorId : IEntityKey
    {
        bool Invoke(IEntityViewControl<TActorId> v,
                    TGameContext context,
                    TActorId entity,
                    ICommand command);

        bool CanHandle(ICommand commandId);
    }
}