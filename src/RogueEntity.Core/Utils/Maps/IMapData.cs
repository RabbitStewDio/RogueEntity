namespace RogueEntity.Core.Utils.Maps
{
    public interface IReadOnlyView2D<T>
    {
        T this[int x, int y] { get; }
    }
    
    public interface IView2D<T>: IReadOnlyView2D<T>
    {
        new T this[int x, int y] { get; set; }
    }
    
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

    public interface IReadOnlyMapData3D<T>
    {
        int Width { get; }
        int Height { get; }
        int Depth { get; }

        T this[int x, int y, int z] { get; }
    }
    
    public interface IMapData3D<T>: IReadOnlyMapData3D<T>
    {
        new T this[int x, int y, int z] { get; set; }
    }
}