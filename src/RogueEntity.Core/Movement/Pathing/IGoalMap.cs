using GoRogue.Pathing;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Movement.Pathing
{
    public interface IGoalMap
    {
        bool TryFindPath(EntityGridPosition origin, out Path<EntityGridPosition> p, out float goalStrengthAtTarget, int maxPathLength = int.MaxValue);
        void AddToGoal(int posX, int posY, float value);

        bool QueryGoalStrength(int x, int y, out float value);
    }
}