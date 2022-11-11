using RogueEntity.Core.MovementPlaning.Goals;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using System;

namespace RogueEntity.Core.MovementPlaning.GoalFinding
{
    public interface IGoalFinder: IDisposable
    {
        public IGoalFinder WithTarget(IGoalFinderTargetSource evaluator);

        public bool TryFindPath<TPosition>(in TPosition source,
                                           out (PathFinderResult resultHint, IPath path, float pathCost) path,
                                           int searchLimit = Int32.MaxValue)
        where TPosition: IPosition<TPosition>;
    }
}
