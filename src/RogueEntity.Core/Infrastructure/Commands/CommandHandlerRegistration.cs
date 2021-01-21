using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Inputs.Commands;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public class CommandHandlerRegistration< TActorId> : ICommandHandlerRegistration< TActorId>
        where TActorId : IEntityKey
    {
        readonly List<ICommandHandler< TActorId>> processors;

        public CommandHandlerRegistration()
        {
            processors = new List<ICommandHandler< TActorId>>();
        }

        public void Register(ICommandHandler< TActorId> p)
        {
            processors.Add(p ?? throw new ArgumentNullException());
        }

        public bool TryGetProcessor(ICommand commandId, out ICommandHandler< TActorId> p)
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