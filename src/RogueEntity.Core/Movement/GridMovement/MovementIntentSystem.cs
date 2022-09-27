using EnTTSharp.Entities;
using RogueEntity.Api.Time;
using System;

namespace RogueEntity.Core.Movement.GridMovement
{
    public class MovementIntentSystem
    {
        readonly Lazy<ITimeSource> timer;

        public MovementIntentSystem(Lazy<ITimeSource> timer)
        {
            this.timer = timer ?? throw new ArgumentNullException(nameof(timer));
        }

        public void ClearMovementIntents<TEntityId>(IEntityViewControl<TEntityId> v, TEntityId k, in MovementIntent mi)
            where TEntityId : struct, IEntityKey
        {
            var endTime = mi.StartTime.Add(TimeSpan.FromSeconds(mi.DurationInSeconds));
            if (endTime > timer.Value.TimeState.FixedGameTimeElapsed)
            {
                v.RemoveComponent<MovementIntent>(k);
            }
        }
    }
}
