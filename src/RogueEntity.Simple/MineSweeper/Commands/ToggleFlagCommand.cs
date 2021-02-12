using RogueEntity.Core.Utils;

namespace RogueEntity.Simple.MineSweeper
{
    public readonly struct ToggleFlagCommand
    {
        public readonly Position2D Position;

        public ToggleFlagCommand(Position2D position)
        {
            Position = position;
        }
    }
}
