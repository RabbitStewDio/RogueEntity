namespace RogueEntity.Core.Utils.DataViews
{
    public interface IView2D<T>: IReadOnlyView2D<T>
    {
        void Clear();
        void Fill(in T value);
        bool TrySet(int x, int y, in T data);
    }
}