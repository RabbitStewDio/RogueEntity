namespace RogueEntity.Core.Inputs.Commands
{
    public static class CommandType
    {
        public static CommandType<TCommand> Of<TCommand>()
        {
            return new CommandType<TCommand>();
        }
    }

    public readonly struct CommandType<TCommand>
    {
        public CommandType<TActor, TCommand> With<TActor>(ICommandHandler<TActor, TCommand> handler)
            => new CommandType<TActor, TCommand>(handler);
    }

    public readonly struct CommandType<TActor, TCommand>
    {
        public readonly ICommandHandler<TActor, TCommand> Handler;

        public CommandType(ICommandHandler<TActor, TCommand> handler)
        {
            this.Handler = handler;
        }
    }
}
