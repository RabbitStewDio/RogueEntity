using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.Commands;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Inputs.Commands
{
    public class CommandReceiver<TActorId> : ICommandReceiver<TActorId>
    {
        readonly Dictionary<Type, ICommandHandler> commandHandlers;

        public CommandReceiver()
        {
            commandHandlers = new Dictionary<Type, ICommandHandler>();
        }

        public void Register<TCommand>([NotNull] ICommandHandler<TActorId, TCommand> handler)
            where TCommand : ICommand
        {
            commandHandlers[typeof(TCommand)] = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public bool TrySubmitCommand<TCommand>(TActorId actor, in TCommand command)
            where TCommand : ICommand
        {
            if (commandHandlers.TryGetValue(typeof(TCommand), out var ch) &&
                ch is ICommandHandler<TActorId, TCommand> handler)
            {
                if (handler.IsValid(actor, command))
                {
                    handler.Submit(actor, command);
                }
            }

            return false;
        }

        public bool IsValid<TCommand>(TActorId actor)
            where TCommand : ICommand
        {
            if (commandHandlers.TryGetValue(typeof(TCommand), out var ch) &&
                ch is ICommandHandler<TActorId, TCommand> handler)
            {
                return handler.IsValid(actor);
            }

            return false;
        }

        public bool IsValid<TCommand>(TActorId actor, in TCommand command)
            where TCommand : ICommand
        {
            if (commandHandlers.TryGetValue(typeof(TCommand), out var ch) &&
                ch is ICommandHandler<TActorId, TCommand> handler)
            {
                return handler.IsValid(actor, command);
            }

            return false;
        }
    }
}
