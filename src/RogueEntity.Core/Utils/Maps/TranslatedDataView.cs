using System;

namespace RogueEntity.Core.Utils.Maps
{
    public readonly struct TranslatedDataView<TData>: IReadOnlyView2D<TData>
    {
        readonly IReadOnlyView2D<TData> data;
        readonly int offsetX;
        readonly int offsetY;

        public TranslatedDataView(IReadOnlyView2D<TData> data, int offsetX, int offsetY)
        {
            this.data = data;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }

        public bool TryGet(int x, int y, out TData dataRaw)
        {
            if (x == 26 && y == 4)
            {
                Console.WriteLine("HERE: ");
            }
            return data.TryGet(x - offsetX, y - offsetY, out dataRaw);
        }

        public TData this[int x, int y]
        {
            get
            {
                if (TryGet(x, y, out TData dataRaw))
                {
                    return dataRaw;
                }

                return default;
            }
        }
    }
}