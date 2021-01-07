using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using System;

namespace RogueEntity.Core.Movement.Goals
{
    [PublicAPI]
    public interface IGoalFinderFilter : IDisposable
    {
        GoalSet FilterGoals(in Position origin, float range, DistanceCalculation dc, GoalSet goals);
    }

    public class PassThroughGoalFinderFilter : IGoalFinderFilter
    {
        public static readonly PassThroughGoalFinderFilter Instance = new PassThroughGoalFinderFilter();
        
        public PassThroughGoalFinderFilter()
        {
        }

        public void Dispose()
        {
        }

        public GoalSet FilterGoals(in Position origin, float range, DistanceCalculation dc, GoalSet receiver)
        {
            return receiver;
        }
    }
}
