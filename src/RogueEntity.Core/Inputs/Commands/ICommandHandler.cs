using RogueEntity.Core.Infrastructure.Commands;

namespace RogueEntity.Core.Inputs.Commands
{
    public interface ICommandHandler<TActor, TCommand> : ICommandHandler
        where TCommand : ICommand
    {
        bool IsValid(TActor actor);
        bool IsValid(TActor actor, TCommand cmd);

        void Submit(TActor actor, in TCommand command);
    }

    /// <summary>
    ///   A tagging interface.
    /// </summary>
    public interface ICommandHandler
    {
    }
}
