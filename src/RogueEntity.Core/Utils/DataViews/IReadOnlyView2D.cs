namespace RogueEntity.Core.Utils.DataViews
{
    public interface IReadOnlyView2D<T>
    {
        bool TryGet(int x, int y, out T data);
        T this[int x, int y] { get; }
    }
}