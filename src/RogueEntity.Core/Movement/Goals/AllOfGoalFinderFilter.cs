using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using System.Collections.Generic;

namespace RogueEntity.Core.Movement.Goals
{
    public class AllOfGoalFinderFilter : IGoalFinderFilter
    {
        readonly List<IGoalFinderFilter> filterSets;
        readonly GoalSet workingSet;
        readonly GoalSet aggregatorSet;

        public AllOfGoalFinderFilter(IReadOnlyList<IGoalFinderFilter> filterSets)
        {
            this.filterSets = new List<IGoalFinderFilter>(filterSets);
            this.workingSet = new GoalSet();
            this.aggregatorSet = new GoalSet();
        }

        public AllOfGoalFinderFilter()
        {
            this.filterSets = new List<IGoalFinderFilter>();
            this.workingSet = new GoalSet();
            this.aggregatorSet = new GoalSet();
        }

        public AllOfGoalFinderFilter With(IGoalFinderFilter f)
        {
            this.filterSets.Add(f);
            return this;
        }

        /// <summary>
        ///    Apply all filters to all underlying sets. Return those positions that were in the result set
        ///    of all filters.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="range"></param>
        /// <param name="dc"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public GoalSet FilterGoals(in Position origin, float range, DistanceCalculation dc, GoalSet receiver)
        {
            aggregatorSet.Clear();
            
            for (var i = 0; i < filterSets.Count; i++)
            {
                workingSet.Clear();
                workingSet.Union(receiver);
                filterSets[i].FilterGoals(origin, range, dc, workingSet);
                if (i == 0)
                {
                    aggregatorSet.Union(workingSet);
                }
                else
                {
                    aggregatorSet.Intersect(workingSet);
                }
            }

            receiver.Clear();
            receiver.Union(aggregatorSet);
            aggregatorSet.Clear();
            workingSet.Clear();
            return receiver;
        }

        public void Dispose()
        {
            foreach (var f in filterSets)
            {
                f.Dispose();
            }
            filterSets.Clear();
            aggregatorSet.Clear();
            workingSet.Clear();
        }
        
    }
}
