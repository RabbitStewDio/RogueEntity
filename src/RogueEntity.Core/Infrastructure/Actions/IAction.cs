using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Actions
{
    public enum ActionResult
    {
        /// <summary>
        ///   The action was performed as intended.
        /// </summary>
        Success,
        /// <summary>
        ///   The action was not performed and no retries should be attempted.
        /// </summary>
        Failure,
        /// <summary>
        ///   The action was not performed but another retry should be attempted.
        /// </summary>
        Pending
    }

    public interface IAction<in TContext, TActorId> where TActorId : IEntityKey
    {
        bool IsMovement { get; }

        /// <summary>
        ///   Represents an activity an actor can perform. 
        ///
        ///   If complete return true, else the same action will execute again at the
        ///   next turn. Use this delaying tactic if there are temporary obstacles that
        ///   you may want to wait out.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        /// <param name="context"></param>
        /// <param name="actionCost">the action-point cost of the activity.</param>
        /// <returns>An indication on whether the action has completed (either successfully or non-successfully)</returns>
        ActionResult Perform(IEntityViewControl<TActorId> v, TContext context, TActorId k, out int actionCost);
    }
}