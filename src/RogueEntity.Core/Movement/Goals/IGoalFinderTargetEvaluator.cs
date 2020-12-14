using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Movement.Goals
{
    public interface IGoalFinderTargetEvaluator: IDisposable
    {
        int CollectGoals(in Position origin, float range, DistanceCalculation dc, IGoalFinderTargetEvaluatorVisitor v);
    }
}
