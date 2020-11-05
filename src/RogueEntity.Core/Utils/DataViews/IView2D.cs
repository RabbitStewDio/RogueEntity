namespace RogueEntity.Core.Utils.DataViews
{
    public interface IView2D<T>: IReadOnlyView2D<T>
    {
        bool TrySet(int x, int y, in T data);
        new T this[int x, int y] { get; set; }
    }
}