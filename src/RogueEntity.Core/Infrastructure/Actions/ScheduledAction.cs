using System;
using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Actions
{
    /// <summary>
    ///  Represents the next scheduled action the character should perform.
    /// </summary>
    public struct ScheduledAction<TContext, TActorId> where TActorId : IEntityKey
    {
        public ScheduledAction(IAction<TContext, TActorId> nextAction, 
                               ActionResult lastActionResult)
        {
            this.NextAction = nextAction ?? throw new ArgumentNullException(nameof(nextAction));
            LastActionResult = lastActionResult;
        }

        public IAction<TContext, TActorId> NextAction { get; }
        public ActionResult LastActionResult { get; }

        public ScheduledAction<TContext, TActorId> WithPreviousResult(ActionResult r)
        {
            return new ScheduledAction<TContext, TActorId>(NextAction, r);
        }
    }
}