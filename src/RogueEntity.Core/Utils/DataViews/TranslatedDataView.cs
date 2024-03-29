using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
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

        public bool TryGet(int x, int y, [MaybeNullWhen(false)] out TData dataRaw)
        {
            return data.TryGet(x - offsetX, y - offsetY, out dataRaw);
        }

        public TData this[int x, int y]
        {
            get
            {
                if (TryGet(x, y, out var dataRaw))
                {
                    return dataRaw;
                }

                return default!;
            }
        }
    }
}