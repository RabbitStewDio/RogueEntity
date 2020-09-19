using System;
using System.Runtime.Serialization;
using EnTTSharp.Entities;
using MessagePack;

namespace RogueEntity.Core.Infrastructure.Actions.Schedule
{
    /// <summary>
    ///  Represents the next scheduled action the character should perform.
    /// </summary>
    [MessagePackObject]
    [DataContract]
    [Serializable]
    public readonly struct ScheduledAction<TContext, TActorId> where TActorId : IEntityKey
    {
        [SerializationConstructor]
        public ScheduledAction(IAction<TContext, TActorId> nextAction, 
                               ActionResult lastActionResult)
        {
            this.NextAction = nextAction ?? throw new ArgumentNullException(nameof(nextAction));
            LastActionResult = lastActionResult;
        }

        [DataMember(Order = 0)]
        [Key(0)]
        public IAction<TContext, TActorId> NextAction { get; }
        [DataMember(Order = 1)]
        [Key(1)]
        public ActionResult LastActionResult { get; }

        public ScheduledAction<TContext, TActorId> WithPreviousResult(ActionResult r)
        {
            return new ScheduledAction<TContext, TActorId>(NextAction, r);
        }
    }
}