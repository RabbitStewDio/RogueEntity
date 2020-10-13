using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning
{
    public readonly struct PositionDirtyEventArgs
    {
        public readonly Position Position;
        public readonly MapLayer Layer;

        public PositionDirtyEventArgs(Position position, MapLayer layer)
        {
            Position = position;
            Layer = layer;
        }
    }
}