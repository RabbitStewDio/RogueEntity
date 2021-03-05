using System;

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
        public static readonly CommandTypeId SharedId = CommandTypeId.Create<TCommand>();
        public CommandTypeId CommandId => SharedId;

        public CommandType<TActor, TCommand> With<TActor>(ICommandHandler<TActor, TCommand> handler)
            => new CommandType<TActor, TCommand>(handler);
    }

    public readonly struct CommandTypeId : IEquatable<CommandTypeId>
    {
        readonly string CommandId;

        public CommandTypeId(string commandId)
        {
            CommandId = commandId;
        }

        public bool Equals(CommandTypeId other)
        {
            return CommandId == other.CommandId;
        }

        public override bool Equals(object obj)
        {
            return obj is CommandTypeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CommandId != null ? CommandId.GetHashCode() : 0);
            }
        }

        public static bool operator ==(CommandTypeId left, CommandTypeId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CommandTypeId left, CommandTypeId right)
        {
            return !left.Equals(right);
        }

        public static CommandTypeId Create<TCommand>()
        {
            return new CommandTypeId(typeof(TCommand).FullName);
        }
    }
    
    public readonly struct CommandType<TActor, TCommand>
    {
        public CommandTypeId CommandId => CommandType<TCommand>.SharedId;
        public readonly ICommandHandler<TActor, TCommand> Handler;

        public CommandType(ICommandHandler<TActor, TCommand> handler)
        {
            this.Handler = handler;
        }
    }
}
