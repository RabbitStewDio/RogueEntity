using RogueEntity.Core.Movement.GoalFinding;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Movement.Goals
{
    public interface IGoalFinderTargetEvaluatorVisitor
    {
        public void RegisterGoalAt<TGoal>(in Position pos, GoalMarker<TGoal> goal);
    }
}
