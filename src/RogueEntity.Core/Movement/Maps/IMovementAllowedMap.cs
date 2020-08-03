using GoRogue;

namespace RogueEntity.Core.Movement.Maps
{
    public interface IMovementAllowedMap
    {
        bool CanMove(int x, int y, Direction d);
    }
}