using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Annotations;
using MessagePack;

namespace RogueEntity.Core.Infrastructure.Commands
{
    [EntityComponent]
    [Serializable]
    [DataContract]
    [MessagePackObject]
    [MessagePackFormatter(typeof(CommandQueueComponentMessagePackFormatter))]
    public class CommandQueueComponent: IReadOnlyCollection<ICommand>
    {
        [DataMember(Order = 0)]
        readonly Queue<ICommand> scheduledCommands;

        public CommandQueueComponent()
        {
            this.scheduledCommands = new Queue<ICommand>();
        }

        public int Count => scheduledCommands.Count;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<ICommand> IEnumerable<ICommand>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Queue<ICommand>.Enumerator GetEnumerator()
        {
            return scheduledCommands.GetEnumerator();
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