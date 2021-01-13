using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.MovementPlaning.Goals.Filters
{
    public class PassThroughGoalFinderFilter : IGoalFinderFilter
    {
        public static readonly PassThroughGoalFinderFilter Instance = new PassThroughGoalFinderFilter();

        public void Dispose()
        {
        }

        public GoalSet FilterGoals(in Position origin, float range, DistanceCalculation dc, GoalSet receiver)
        {
            return receiver;
        }
    }
}
