using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using System;

namespace RogueEntity.Core.MovementPlaning.Goals.Filters
{
    public class TopNGoalFinderFilter: IGoalFinderFilter
    {
        readonly int numberOfElements;
        readonly BufferList<GoalRecord> sourceBuffer;
        readonly GoalSet workingBuffer;

        public TopNGoalFinderFilter(int numberOfElements)
        {
            this.numberOfElements = numberOfElements;
            sourceBuffer = new BufferList<GoalRecord>();
            workingBuffer = new GoalSet();
        }

        public GoalSet FilterGoals(in Position origin, float range, DistanceCalculation dc, GoalSet goals)
        {
            goals.CopyTo(sourceBuffer);
            goals.Clear();
            Array.Sort(sourceBuffer.Data, 0, sourceBuffer.Count, GoalRecord.StrengthComparer);
            var limit = Math.Min(sourceBuffer.Count, numberOfElements);
            for (var i = 0; i < limit; i++)
            {
                goals.Add(sourceBuffer[i]);
            }
            return goals;
        }
        
        public void Dispose()
        {
            sourceBuffer.Clear();
            workingBuffer.Clear();
        }

    }
}
