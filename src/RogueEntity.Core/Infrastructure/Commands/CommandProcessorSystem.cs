using System;
using System.Collections.Generic;
using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Actions;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Commands
{
    public class CommandProcessorSystem<TGameContext, TActorId> : ICommandProcessorRegistration<TGameContext, TActorId>, 
                                                                  ICommandProcessor<TGameContext, TActorId> 
        where TActorId : IEntityKey
    {
        readonly ILogger logger = SLog.ForContext<CommandProcessorSystem<TGameContext, TActorId>>();
        readonly List<ICommandHandler<TGameContext, TActorId>> processors;

        public CommandProcessorSystem()
        {
            processors = new List<ICommandHandler<TGameContext, TActorId>>();
        }

        ICommandProcessor<TGameContext, TActorId> ICommandProcessorRegistration<TGameContext, TActorId>.Processor => this;

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

        public void ProcessActions(IEntityViewControl<TActorId> v, 
                                   TGameContext context,
                                   TActorId entity,
                                   in CommandQueueComponent commandQueue,
                                   in IdleMarker idleConstraint)
        {
            if (!commandQueue.TryGet(out var command))
            {
                return;
            }

            if (!TryGetProcessor(command, out var processor))
            {
                logger.Warning("No processor for command {CommandId}", command.Id);
                return;
            }

            processor.Invoke(v, context, entity, command);
            if (!v.GetComponent<ScheduledActionPlan<TGameContext, TActorId>>(entity, out var ap) ||
                ap.Empty)
            {
                logger.Information("Processor rejected command {CommandId}", command.Id);
            }
        }
    }
}