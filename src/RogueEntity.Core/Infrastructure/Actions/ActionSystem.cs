using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Actions
{
    public class ActionSystem<TGameContext, TActorId>
        where TActorId : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<ActionSystem<TGameContext, TActorId>>();

        protected virtual bool Ready(IEntityViewControl<TActorId> v, TActorId k, TGameContext context, in ActionPoints points)
        {
            return points.CanPerformActions();
        }

        protected virtual bool Ready(IEntityViewControl<TActorId> v, TActorId k, TGameContext context, in MovementPoints points)
        {
            return points.CanPerformActions();
        }

        public virtual void FetchNextAction(IEntityViewControl<TActorId> v, TGameContext context, TActorId k,
                                            in ScheduledActionPlan<TGameContext, TActorId> plan,
                                            in IdleMarker idleMarker)
        {
            if (plan.TryDequeue(out var action))
            {
                v.AssignComponent(k, action);
                v.RemoveComponent<IdleMarker>(k);
            }
        }

        public virtual void MaintainActionPointStatus(IEntityViewControl<TActorId> v, 
                                                      TGameContext context, 
                                                      TActorId k,
                                                      in ActionPoints ap)
        {
            if (ap.CanPerformActions())
            {
                v.GetOrCreateComponent<TActorId, ActionReadyMarker>(k);
            }
            else
            {
                v.RemoveComponent<ActionReadyMarker>(k);
            }
        }

        public virtual void MaintainMovementPointStatus(IEntityViewControl<TActorId> v, 
                                                        TGameContext context, 
                                                        TActorId k,
                                                        in MovementPoints ap)
        {
            if (ap.CanPerformActions())
            {
                v.GetOrCreateComponent<TActorId, MovementReadyMarker>(k);
            }
            else
            {
                v.RemoveComponent<MovementReadyMarker>(k);
            }
        }

        /// <summary>
        ///   Runs an action system that combines movement and actions into a
        ///   single measure. Performing either a move or an action costs action
        ///   points.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="action"></param>
        public virtual void RunUnifiedSystem(IEntityViewControl<TActorId> v,
                                             TGameContext context,
                                             TActorId k,
                                             in ScheduledAction<TGameContext, TActorId> action)
        {
            var nextAction = action;
            var actionCost = 0;
            while (actionCost == 0)
            {
                if (!RunAction(v, context, k, in nextAction, out actionCost, out var actionResult))
                {
                    return;
                }

                if (actionResult == ActionResult.Pending)
                {
                    return;
                }

                if (v.GetComponent(k, out ScheduledActionPlan<TGameContext, TActorId> plan) && plan.TryDequeue(out nextAction))
                {
                    v.ReplaceComponent(k, nextAction.WithPreviousResult(actionResult));
                    v.RemoveComponent<IdleMarker>(k);
                }
                else
                {
                    v.RemoveComponent<ScheduledAction<TGameContext, TActorId>>(k);
                    v.AssignOrReplace<IdleMarker>(k);
                    return;
                }
            }
        }

        /// <summary>
        ///   Runs an action system that separates action points from movement points.
        ///   This is useful if you want actions to have a separate cool down from movements.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="context"></param>
        /// <param name="k"></param>
        /// <param name="action"></param>
        public virtual void RunSplitSystem(IEntityViewControl<TActorId> v,
                                           TGameContext context,
                                           TActorId k,
                                           in ScheduledAction<TGameContext, TActorId> action)
        {
            var nextAction = action;
            var actionCost = 0;
            while (actionCost == 0)
            {
                ActionResult actionResult;
                if (nextAction.NextAction.IsMovement)
                {
                    if (!RunMovement(v, context, k, in nextAction, out actionCost, out actionResult))
                    {
                        return;
                    }
                }
                else
                {
                    if (!RunAction(v, context, k, in nextAction, out actionCost, out actionResult))
                    {
                        return;
                    }
                }

                if (actionResult == ActionResult.Pending)
                {
                    return;
                }

                if (v.GetComponent(k, out ScheduledActionPlan<TGameContext, TActorId> plan) && 
                    plan.TryDequeue(out nextAction))
                {
                    v.ReplaceComponent(k, nextAction.WithPreviousResult(actionResult));
                    v.RemoveComponent<IdleMarker>(k);
                }
                else
                {
                    v.RemoveComponent<ScheduledAction<TGameContext, TActorId>>(k);
                    v.AssignOrReplace<IdleMarker>(k);
                    return;
                }
            }
        }

        bool RunMovement(IEntityViewControl<TActorId> v, TGameContext context, TActorId k,
                         in ScheduledAction<TGameContext, TActorId> action,
                         out int actionCost,
                         out ActionResult actionResult)
        {
            actionCost = 0;
            if (!v.GetComponent(k, out MovementPoints points))
            {
                actionResult = default;
                return false;
            }

            if (!Ready(v, k, context, in points))
            { 
                Logger.Debug("{Entity}:{points} - Not ready to run any movement.", k, points);
                actionResult = default;
                return false;
            }

            actionResult = action.NextAction.Perform(v, context, k, out actionCost);
            Logger.Debug("{Entity}:{points} - Running {Action} with {cost} movement point cost and resulted {ActionResult}",
                         k, points, action.NextAction, actionCost, actionResult);

            if (actionCost != 0)
            {
                var actionPoints = points.Spend(actionCost);
                v.ReplaceComponent(k, actionPoints);
            }
            return true;
        }

        bool RunAction(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, in ScheduledAction<TGameContext, TActorId> action,
                       out int actionCost,
                       out ActionResult actionResult)
        {
            actionCost = 0;
            if (!v.GetComponent(k, out ActionPoints points))
            {
                actionResult = default;
                return false;
            }

            if (!Ready(v, k, context, in points))
            {
                Logger.Debug("{Entity}:{points} - Not ready to run any action.", k, points);
                actionResult = default;
                return false;
            }

            actionResult = action.NextAction.Perform(v, context, k, out actionCost);
            Logger.Debug("{Entity}:{points} - Running {Action} with {cost} action point cost and resulted {actionResult}",
                         k, points, action.NextAction, actionCost, actionResult);

            if (actionCost != 0)
            {
                var actionPoints = points.Spend(actionCost);
                v.ReplaceComponent(k, actionPoints);
            }
            return (actionResult != ActionResult.Pending);
        }
    }

    /*
    public static class ActionSystem
    {
        public static bool IsUnifiedActionSystem<TGameContext>(this TGameContext context)
            where TGameContext : IGameContext
        {
            return context.Configuration.HasFlag(CoreModule.CoreModule.UnifiedActionSystemProperty);
        }

        public static bool IsReadyToAct<TGameContext, TActorId>(this TGameContext context, TActorId actor)
            where TGameContext : IGameContext, IMapContext, IGameContext<TGameContext>, IFactionContext
        {
            if (context.ActorResolver.TryQueryData(actor, context, out ActionPoints ap))
            {
                return ap.CanPerformActions();
            }

            // not constrained by action points.
            return true;
        }

        public static bool IsReadyToMove<TGameContext, TActorId>(this TGameContext context, TActorId actor)
            where TGameContext : IGameContext, IMapContext, IGameContext<TGameContext>, IFactionContext
        {
            if (context.IsUnifiedActionSystem())
            {
                return context.IsReadyToAct(actor);
            }

            if (context.ActorResolver.TryQueryData(actor, context, out MovementPoints ap))
            {
                return ap.CanPerformActions();
            }

            // not constrained by action points.
            return true;
        }
    }
    */
}