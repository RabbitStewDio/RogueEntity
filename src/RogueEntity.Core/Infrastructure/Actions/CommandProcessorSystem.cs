using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Actions.Schedule;
using RogueEntity.Core.Infrastructure.Commands;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Actions
{
    public class CommandProcessorSystem<TActorId>
        where TActorId : IEntityKey
    {
        readonly ILogger logger = SLog.ForContext<CommandProcessorSystem<TActorId>>();
        readonly ICommandHandlerRegistration<TActorId> commandHandler;

        public CommandProcessorSystem(ICommandHandlerRegistration<TActorId> commandHandler)
        {
            this.commandHandler = commandHandler;
        }

        public void ProcessActions(IEntityViewControl<TActorId> v,
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

            processor.Invoke(v, entity, command);
            if (!v.GetComponent<ScheduledActionPlan<TActorId>>(entity, out var ap) ||
                ap.Empty)
            {
                logger.Information("Processor rejected command {CommandId}", command.Id);
            }
        }
    }
}
