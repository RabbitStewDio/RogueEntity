using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Infrastructure.Commands
{
    [Serializable]
    public class CommandQueueComponent
    {
        readonly Queue<ICommand> scheduledCommands;

        public CommandQueueComponent()
        {
            this.scheduledCommands = new Queue<ICommand>();
        }

        public void PerformNow(ICommand command)
        {
            this.scheduledCommands.Clear();
            this.scheduledCommands.Enqueue(command);
        }

        public void PerformLater(ICommand command)
        {
            this.scheduledCommands.Enqueue(command);
        }

        public bool TryGet(out ICommand command)
        {
            if (scheduledCommands.Count == 0)
            {
                command = default;
                return false;
            }

            command = scheduledCommands.Dequeue();
            return true;
        }

        public bool Empty => scheduledCommands.Count == 0;
    }
}