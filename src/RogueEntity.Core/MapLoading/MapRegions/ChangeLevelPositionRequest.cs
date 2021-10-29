using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public readonly struct ChangeLevelPositionRequest
    {
        public readonly Position Position;

        public ChangeLevelPositionRequest(Position position)
        {
            this.Position = position;
        }
    }
}
