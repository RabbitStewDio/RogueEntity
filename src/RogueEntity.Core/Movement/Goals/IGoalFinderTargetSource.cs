using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Movement.Goals
{
    public interface IGoalFinderTargetSource : IDisposable
    {
        GoalSet CollectGoals(in Position origin, float range, DistanceCalculation dc, GoalSet receiver);
    }
}
