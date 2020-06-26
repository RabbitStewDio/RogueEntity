using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Actions
{
    public static class SchedulerExtensions
    {
        public static void ClearSchedule<TGameContext, TActorId>(this TGameContext context, TActorId actor)
            where TGameContext : IItemContext<TGameContext, TActorId>
            where TActorId : IEntityKey
        {
            if (context.ItemResolver.TryQueryData(actor, context, out ScheduledActionPlan<TGameContext, TActorId> planner))
            {
                planner.DiscardAll();
            }
        }

        public static void ScheduleAction<TGameContext, TActorId>(this TGameContext context, TActorId actor,
                                                                  IAction<TGameContext, TActorId> action,
                                                                  ActionResult lastResult = ActionResult.Success)
            where TGameContext : IItemContext<TGameContext, TActorId>
            where TActorId : IEntityKey
        {
            if (!context.ItemResolver.TryQueryData(actor, context, out ScheduledActionPlan<TGameContext, TActorId> planer))
            {
                Log.Warning("Attempted to schedule an action for actor {Actor} that has no scheduler", actor);
                return;
            }

            planer.Add(new ScheduledAction<TGameContext, TActorId>(action, lastResult));
        }

        public static void RunNext<TGameContext, TActorId>(this TGameContext context, TActorId actor,
                                                           IAction<TGameContext, TActorId> action,
                                                           ActionResult lastResult = ActionResult.Success)
            where TGameContext : IItemContext<TGameContext, TActorId> 
            where TActorId : IEntityKey
        {
            if (!context.ItemResolver.TryQueryData(actor, context, out ScheduledActionPlan<TGameContext, TActorId> planer))
            {
                Log.Warning("Attempted to schedule an action for actor {Actor} that has no scheduler", actor);
                return;
            }

            planer.DiscardAll();
            planer.Add(new ScheduledAction<TGameContext, TActorId>(action, lastResult));
        }
    }
}