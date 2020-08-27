namespace RogueEntity.Core.Positioning
{
    public interface IMapBoundsContext
    {
        MapBoundary MapExtent { get; }
    }

    public readonly struct MapBoundary
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;
    }
}