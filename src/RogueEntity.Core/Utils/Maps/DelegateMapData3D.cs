using System;

namespace RogueEntity.Core.Utils.Maps
{
    /// <summary>
    ///   Delegates map data access to a separate function or data set.
    ///   Objects of this type should never be serialized, as function
    ///   references do not travel well in serialized data streams.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class DelegateMapData3D<TData>: IReadOnlyMapData3D<TData>
    {
        readonly Func<int, int, int, TData> query;
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }

        public DelegateMapData3D(int width, int height, int depth, Func<int, int, int, TData> query)
        {
            this.query = query;
            Width = width;
            Height = height;
            Depth = depth;
        }

        public TData this[int x, int y, int z]
        {
            get => query(x, y, z);

        }
    }
}