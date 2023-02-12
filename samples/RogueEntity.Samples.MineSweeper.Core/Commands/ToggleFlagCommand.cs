using RogueEntity.Core.Utils;

namespace RogueEntity.Samples.MineSweeper.Core.Commands
{
    public readonly struct ToggleFlagCommand
    {
        public readonly GridPosition2D Position;

        public ToggleFlagCommand(GridPosition2D position)
        {
            Position = position;
        }
    }
}
