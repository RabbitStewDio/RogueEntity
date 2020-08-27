using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Actions
{
    public class CommandProcessorSystem<TGameContext, TActorId>
        where TActorId : IEntityKey
    {
        readonly ILogger logger = SLog.ForContext<CommandProcessorSystem<TGameContext, TActorId>>();
        readonly ICommandHandlerRegistration<TGameContext, TActorId> commandHandler;

        public CommandProcessorSystem(ICommandHandlerRegistration<TGameContext, TActorId> commandHandler)
        {
            this.commandHandler = commandHandler;
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

            if (!commandHandler.TryGetProcessor(command, out var processor))
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