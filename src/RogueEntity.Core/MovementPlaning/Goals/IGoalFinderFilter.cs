using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using System;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    [PublicAPI]
    public interface IGoalFinderFilter : IDisposable
    {
        GoalSet FilterGoals(in Position origin, float range, DistanceCalculation dc, GoalSet goals);
    }
}
