using RogueEntity.Core.Infrastructure.Commands;

namespace RogueEntity.Core.Inputs.Commands
{
    /// <summary>
    ///  A per-player entity service.
    /// </summary>
    public interface ICommandReceiver<TActorId>
    {
        bool TrySubmitCommand<TCommand>(TActorId actor, in TCommand command)
            where TCommand : ICommand;

        /// <summary>
        ///   Checks whether there is at least one command of this type that could be executed.
        ///   Use this to generally signal whether a given command is valid. A paralyzed player
        ///   might for instance reject all move commands.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        bool IsValid<TCommand>(TActorId actor)
            where TCommand : ICommand;

        /// <summary>
        ///   Checks whether the given command instance could possibly be executed.
        /// 
        ///   Use this to validate command options.
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        bool IsValid<TCommand>(TActorId actor, in TCommand command)
            where TCommand : ICommand;
    }
}
