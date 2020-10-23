namespace RogueEntity.Core.Positioning
{
    public readonly struct PositionDirtyEventArgs
    {
        public readonly Position Position;

        public PositionDirtyEventArgs(Position position)
        {
            Position = position;
        }
    }
}