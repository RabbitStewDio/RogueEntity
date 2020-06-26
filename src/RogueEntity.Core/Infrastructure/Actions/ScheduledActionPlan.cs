using System.Collections.Generic;
using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Actions
{
    public class ScheduledActionPlan<TContext, TActorId> where TActorId : IEntityKey
    {
        readonly Queue<ScheduledAction<TContext, TActorId>> additionalActions;

        public ScheduledActionPlan()
        {
            additionalActions = new Queue<ScheduledAction<TContext, TActorId>>();
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