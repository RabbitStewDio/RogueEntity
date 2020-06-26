using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Commands
{
    /// <summary>
    ///   Takes a command and translates it into an entity-system
    ///   state.
    /// </summary>
    public interface ICommandHandler<in TGameContext, TActorId> where TActorId : IEntityKey
    {
        void Invoke(IEntityViewControl<TActorId> v,
                    TGameContext context,
                    TActorId entity,
                    ICommand command);

        bool CanHandle(ICommand commandId);
    }
}