using System.Collections.Generic;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Movement.Goals
{
    public class CompoundGoalTargetSource : IGoalFinderTargetSource
    {
        readonly List<IGoalFinderTargetSource> entityTypeGoalFinders;

        public CompoundGoalTargetSource()
        {
            entityTypeGoalFinders = new List<IGoalFinderTargetSource>();
        }

        public void Dispose()
        {
            foreach (var e in entityTypeGoalFinders)
            {
                e.Dispose();
            }
            entityTypeGoalFinders.Clear();
        }

        public void Add(IGoalFinderTargetSource e)
        {
            entityTypeGoalFinders.Add(e);
        }

        public GoalSet CollectGoals(in Position origin, float range, DistanceCalculation dc, GoalSet receiver)
        {
            var buffer = GoalSet.PrepareBuffer(receiver);

            for (var index = 0; index < entityTypeGoalFinders.Count; index++)
            {
                var f = entityTypeGoalFinders[index];
                f.CollectGoals(origin, range, dc, buffer);
            }

            return buffer;
            
        }
    }
}