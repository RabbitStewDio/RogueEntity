using System;

namespace RogueEntity.Core.Utils.Maps
{
    /// <summary>
    ///   Delegates map data access to a separate function or data set.
    ///   Objects of this type should never be serialized, as function
    ///   references do not travel well in serialized data streams.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class DelegateMapData<TData>: IReadOnlyMapData<TData>
    {
        readonly Func<int, int, TData> query;
        public int Width { get; }
        public int Height { get; }

        public DelegateMapData(int width, int height, Func<int, int, TData> query)
        {
            this.query = query;
            Width = width;
            Height = height;
        }

        public TData this[int x, int y]
        {
            get => query(x, y);

        }
    }
}