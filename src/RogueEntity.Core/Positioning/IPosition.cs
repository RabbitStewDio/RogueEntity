namespace RogueEntity.Core.Positioning
{
    public interface IPosition
    {
        double X { get; }
        double Y { get; }
        double Z { get; }

        int GridX { get; }
        int GridY { get; }
        int GridZ { get; }

        byte LayerId { get; }
        bool IsInvalid { get; }
    }
}