using RogueEntity.Core.Utils;

namespace RogueEntity.Samples.MineSweeper.Core.Commands
{
    public readonly struct RevealMapPositionCommand
    {
        public readonly GridPosition2D Position;

        public RevealMapPositionCommand(GridPosition2D position)
        {
            Position = position;
        }
    }
}
