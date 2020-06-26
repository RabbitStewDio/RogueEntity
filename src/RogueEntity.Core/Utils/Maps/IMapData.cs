namespace RogueEntity.Core.Utils.Maps
{
    public interface IMapData<T>: IReadOnlyMapData<T>
    {
        new T this[int x, int y] { get; set; }
    }

    public interface IReadOnlyMapData<T>
    {
        int Width { get; }
        int Height { get; }

        T this[int x, int y] { get; }
    }
}