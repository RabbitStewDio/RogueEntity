using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.MovementPlaning.Goals.Filters
{
    public abstract class SelectManyGoalFinderFilter: IGoalFinderFilter
    {
        readonly BufferList<GoalRecord> sourceBuffer;
        readonly GoalSet workingBuffer;

        protected SelectManyGoalFinderFilter()
        {
            sourceBuffer = new BufferList<GoalRecord>();
            workingBuffer = new GoalSet();
        }

        public GoalSet FilterGoals(in Position origin, float range, DistanceCalculation dc, GoalSet goals)
        {
            goals.CopyTo(sourceBuffer);
            goals.Clear();
            foreach (var goal in sourceBuffer)
            {
                workingBuffer.Clear();
                ProcessGoal(origin, range, dc, goal, workingBuffer);
                goals.Union(workingBuffer);
            }

            return goals;
        }

        protected abstract void ProcessGoal(in Position origin, float range, DistanceCalculation dc, GoalRecord goal, GoalSet resultBuffer);
        
        public void Dispose()
        {
            sourceBuffer.Clear();
            workingBuffer.Clear();
        }

    }
}
