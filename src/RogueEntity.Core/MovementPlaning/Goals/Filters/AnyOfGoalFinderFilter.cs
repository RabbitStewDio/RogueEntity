using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Goals.Filters
{
    public class AnyOfGoalFinderFilter : IGoalFinderFilter
    {
        readonly List<IGoalFinderFilter> filterSets;
        readonly GoalSet workingSet;
        readonly GoalSet aggregateSet;

        public AnyOfGoalFinderFilter(IReadOnlyList<IGoalFinderFilter> filterSets)
        {
            this.filterSets = new List<IGoalFinderFilter>(filterSets);
            this.workingSet = new GoalSet();
            this.aggregateSet = new GoalSet();
        }

        public AnyOfGoalFinderFilter()
        {
            this.filterSets = new List<IGoalFinderFilter>();
            this.workingSet = new GoalSet();
            this.aggregateSet = new GoalSet();
        }

        public AnyOfGoalFinderFilter With(IGoalFinderFilter f)
        {
            this.filterSets.Add(f);
            return this;
        }

        public GoalSet FilterGoals(in Position origin, float range, DistanceCalculation dc, GoalSet receiver)
        {
            aggregateSet.Clear();
            
            for (var i = 0; i < filterSets.Count; i++)
            {
                workingSet.Clear();
                workingSet.Union(receiver);
                filterSets[i].FilterGoals(origin, range, dc, workingSet);
                aggregateSet.Union(workingSet);
            }

            receiver.Clear();
            receiver.Union(aggregateSet);
            return receiver;
        }

        public void Dispose()
        {
            foreach (var f in filterSets)
            {
                f.Dispose();
            }
            filterSets.Clear();
        }
        
    }
}
