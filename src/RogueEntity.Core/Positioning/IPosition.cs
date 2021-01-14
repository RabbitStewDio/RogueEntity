namespace RogueEntity.Core.Positioning
{
    public interface IPosition<out TPosition> where TPosition: IPosition<TPosition> 
    {
        double X { get; }
        double Y { get; }
        double Z { get; }

        int GridX { get; }
        int GridY { get; }
        int GridZ { get; }

        byte LayerId { get; }
        bool IsInvalid { get; }

        TPosition WithPosition(int x, int y);
        TPosition WithPosition(double tx, double ty);
    }

    public interface IPositionChangeMarker<out TPosition>
        where TPosition : IPosition<TPosition>
    {
        TPosition PreviousPosition { get; }
    }
}