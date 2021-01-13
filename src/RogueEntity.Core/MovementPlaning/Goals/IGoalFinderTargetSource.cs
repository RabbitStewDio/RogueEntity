using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using System;

namespace RogueEntity.Core.MovementPlaning.Goals
{
    public interface IGoalFinderTargetSource : IDisposable
    {
        GoalSet CollectGoals(in Position origin, float range, DistanceCalculation dc, GoalSet receiver);
    }
}
