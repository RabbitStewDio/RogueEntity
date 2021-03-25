﻿using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.Time;
using System;

namespace RogueEntity.Core.Movement.GridMovement
{
    public class MovementIntentSystem
    {
        readonly Lazy<ITimeSource> timer;

        public MovementIntentSystem([NotNull] Lazy<ITimeSource> timer)
        {
            this.timer = timer ?? throw new ArgumentNullException(nameof(timer));
        }

        public void ClearMovementIntents<TEntityId>(IEntityViewControl<TEntityId> v, TEntityId k, in MovementIntent mi)
            where TEntityId : IEntityKey
        {
            var endTime = mi.StartTime.Add(TimeSpan.FromSeconds(mi.DurationInSeconds));
            if (endTime > timer.Value.CurrentTime)
            {
                v.RemoveComponent<MovementIntent>(k);
            }
        }
    }
}