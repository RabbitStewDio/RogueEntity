using System;

namespace RogueEntity.Core.Utils.Maps
{
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