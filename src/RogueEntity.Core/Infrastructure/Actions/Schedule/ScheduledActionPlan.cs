using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Actions.Schedule
{
    [DataContract]
    [Serializable]
    public class ScheduledActionPlan<TActorId> : IReadOnlyCollection<ScheduledAction<TActorId>>
        where TActorId : IEntityKey
    {
        readonly Queue<ScheduledAction<TActorId>> additionalActions;

        public ScheduledActionPlan()
        {
            additionalActions = new Queue<ScheduledAction<TActorId>>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<ScheduledAction<TActorId>> IEnumerable<ScheduledAction<TActorId>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => additionalActions.Count;

        public Queue<ScheduledAction<TActorId>>.Enumerator GetEnumerator()
        {
            return additionalActions.GetEnumerator();
        }

        public void DiscardAll()
        {
            additionalActions.Clear();
        }

        public void Add(ScheduledAction<TActorId> action)
        {
            additionalActions.Enqueue(action);
        }

        public bool Empty => additionalActions.Count == 0;

        public bool TryDequeue(out ScheduledAction<TActorId> action)
        {
            if (additionalActions.Count > 0)
            {
                action = additionalActions.Dequeue();
                return true;
            }

            action = default;
            return false;
        }
    }
}
