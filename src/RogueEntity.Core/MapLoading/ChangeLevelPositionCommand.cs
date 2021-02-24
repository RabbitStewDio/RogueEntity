using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.MapLoading
{
    public readonly struct ChangeLevelPositionCommand
    {
        public readonly Position Position;

        public ChangeLevelPositionCommand(Position position)
        {
            this.Position = position;
        }
    }
}
