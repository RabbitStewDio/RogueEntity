using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
{
    public interface IReadOnlyView2D<T>
    {
        bool TryGet(int x, int y, [MaybeNullWhen(false)] out T data);
        T this[int x, int y] { get; }
    }
}