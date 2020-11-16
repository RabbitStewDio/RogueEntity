using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public class CommandHandlerRegistration<TGameContext, TActorId> : ICommandHandlerRegistration<TGameContext, TActorId>
        where TActorId : IEntityKey
    {
        readonly ILogger logger = SLog.ForContext<CommandHandlerRegistration<TGameContext, TActorId>>();
        readonly List<ICommandHandler<TGameContext, TActorId>> processors;

        public CommandHandlerRegistration()
        {
            processors = new List<ICommandHandler<TGameContext, TActorId>>();
        }

        public void Register(ICommandHandler<TGameContext, TActorId> p)
        {
            processors.Add(p ?? throw new ArgumentNullException());
        }

        public bool TryGetProcessor(ICommand commandId, out ICommandHandler<TGameContext, TActorId> p)
        {
            foreach (var pr in processors)
            {
                if (pr.CanHandle(commandId))
                {
                    p = pr;
                    return true;
                }
            }

            p = default;
            return false;
        }
    }
}