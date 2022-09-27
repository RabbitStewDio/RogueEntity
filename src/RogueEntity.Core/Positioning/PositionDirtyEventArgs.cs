using EnTTSharp;
using RogueEntity.Core.Utils;
using System;

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
    
    public readonly struct MapRegionDirtyEventArgs
    {
        public readonly int ZPositionFrom;
        public readonly int ZPositionTo;
        public readonly Optional<Rectangle> LayerArea;

        public MapRegionDirtyEventArgs(int zPositionFrom, int zPositionTo, Optional<Rectangle> layerArea = default)
        {
            ZPositionFrom = Math.Min(zPositionFrom, zPositionTo);
            ZPositionTo = Math.Max(zPositionFrom, zPositionTo);
            LayerArea = layerArea;
        }

        public bool IsSingleLayer => ZPositionFrom == ZPositionTo;
    }
}