using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Actions
{
    public static class PointRecoveryTimeSystem
    {
        static readonly ILogger Logger = SLog.ForContext(typeof(PointRecoveryTimeSystem));

        public static void Update<TGameContext, TActorId>(IEntityViewControl<TActorId> v, TGameContext context, 
                                                          TActorId key, 
                                                          in ActionPointRecoveryTime timer,
                                                          in ActionPoints actionPoints) 
            where TGameContext: ITimeContext 
            where TActorId : IEntityKey
        {
            var turn = context.TimeSource.FixedStepTime;
            if (timer.IsReady(turn) && 
                actionPoints.TryRecover(timer.ActionPointsRecovery, out var next))
            {
                var time = timer.Recover(turn);
                Logger.Verbose("{Entity} recovered {Points} and now has {CurrentBalance}", key, timer.ActionPointsRecovery, next);

                if (next != actionPoints)
                {
                    v.WriteBack(key, in next);
                }
                v.WriteBack(key, in time);
            }
            else
            {
                Logger.Verbose("{Entity} skipped recovery", key);
            }
        }

        public static void Update<TGameContext, TActorId>(IEntityViewControl<TActorId> v, TGameContext context,
                                                          TActorId key, 
                                                          in MovementPointRecoveryTime timer,
                                                          in MovementPoints movementPoints) 
            where TGameContext: ITimeContext 
            where TActorId : IEntityKey
        {
            var turn = context.TimeSource.FixedStepTime;
            if (timer.IsReady(turn) && 
                movementPoints.TryRecover(timer.MovementPointsRecovery, out var next))
            {
                var time = timer.Recover(turn);

                Logger.Verbose("{Entity} recovered {Points} and now has {CurrentBalance}", key, timer.MovementPointsRecovery, next);

                if (next != movementPoints)
                {
                    v.WriteBack(key, in next);
                }
                v.WriteBack(key, in time);
            }
            else
            {
                Logger.Verbose("{Entity} skipped recovery", key);
            }
        }
    }
}