namespace RogueEntity.Core.Utils.DataViews;

public interface IReadOnlyBoundedDataView<TData> : IReadOnlyView2D<TData>
{
    Rectangle Bounds { get; }
    bool Contains(int x, int y);
}