using RogueEntity.Core.Utils;

namespace RogueEntity.Simple.MineSweeper
{
    public readonly struct RevealMapPositionCommand
    {
        public readonly Position2D Position;

        public RevealMapPositionCommand(Position2D position)
        {
            Position = position;
        }
    }
}
