using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Actions.Schedule
{
    public static class SchedulerExtensions
    {
        public static void ClearSchedule<TActorId>(this IItemResolver<TActorId> context, TActorId actor)
            where TActorId : IEntityKey
        {
            if (context.TryQueryData(actor, out ScheduledActionPlan<TActorId> planner))
            {
                planner.DiscardAll();
            }
        }

        public static void ScheduleAction<TActorId>(this IItemResolver<TActorId> context,
                                                    TActorId actor,
                                                    IAction<TActorId> action,
                                                    ActionResult lastResult = ActionResult.Success)
            where TActorId : IEntityKey
        {
            if (!context.TryQueryData(actor, out ScheduledActionPlan<TActorId> planer))
            {
                Log.Warning("Attempted to schedule an action for actor {Actor} that has no scheduler", actor);
                return;
            }

            planer.Add(new ScheduledAction<TActorId>(action, lastResult));
        }

        public static void RunNext<TActorId>(this IItemResolver<TActorId> context,
                                             TActorId actor,
                                             IAction<TActorId> action,
                                             ActionResult lastResult = ActionResult.Success)
            where TActorId : IEntityKey
        {
            if (!context.TryQueryData(actor, out ScheduledActionPlan<TActorId> planer))
            {
                Log.Warning("Attempted to schedule an action for actor {Actor} that has no scheduler", actor);
                return;
            }

            planer.DiscardAll();
            planer.Add(new ScheduledAction<TActorId>(action, lastResult));
        }
    }
}
