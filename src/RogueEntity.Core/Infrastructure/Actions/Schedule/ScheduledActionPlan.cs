using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EnTTSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Actions
{
    [DataContract]
    [Serializable]
    public class ScheduledActionPlan<TContext, TActorId>: IReadOnlyCollection<ScheduledAction<TContext, TActorId>> where TActorId : IEntityKey
    {
        readonly Queue<ScheduledAction<TContext, TActorId>> additionalActions;
        
        public ScheduledActionPlan()
        {
            additionalActions = new Queue<ScheduledAction<TContext, TActorId>>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<ScheduledAction<TContext, TActorId>> IEnumerable<ScheduledAction<TContext, TActorId>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => additionalActions.Count;

        public Queue<ScheduledAction<TContext, TActorId>>.Enumerator GetEnumerator()
        {
            return additionalActions.GetEnumerator();
        }

        public void DiscardAll()
        {
            additionalActions.Clear();
        }

        public void Add(ScheduledAction<TContext, TActorId> action)
        {
            additionalActions.Enqueue(action);
        }

        public bool Empty => additionalActions.Count == 0;

        public bool TryDequeue(out ScheduledAction<TContext, TActorId> action)
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