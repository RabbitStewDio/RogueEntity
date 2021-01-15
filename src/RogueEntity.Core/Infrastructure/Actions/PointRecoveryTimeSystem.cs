using EnTTSharp.Entities;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using Serilog;

namespace RogueEntity.Core.Infrastructure.Actions
{
    public class PointRecoveryTimeSystem
    {
        static readonly ILogger Logger = SLog.ForContext(typeof(PointRecoveryTimeSystem));
        ITimeContext timeContext;

        public PointRecoveryTimeSystem(ITimeContext timeContext)
        {
            this.timeContext = timeContext;
        }

        public void Update<TActorId>(IEntityViewControl<TActorId> v,
                                            TActorId key,
                                            in ActionPointRecoveryTime timer,
                                            in ActionPoints actionPoints)
            where TActorId : IEntityKey
        {
            var turn = timeContext.TimeSource.FixedStepTime;
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

        public void Update<TActorId>(IEntityViewControl<TActorId> v,
                                            TActorId key,
                                            in MovementPointRecoveryTime timer,
                                            in MovementPoints movementPoints)
            where TActorId : IEntityKey
        {
            var turn = timeContext.TimeSource.FixedStepTime;
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
