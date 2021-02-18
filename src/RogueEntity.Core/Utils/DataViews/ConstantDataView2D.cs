namespace RogueEntity.Core.Utils.DataViews
{
    public class ConstantDataView2D<TData>: IReadOnlyView2D<TData>
    {
        readonly TData value;

        public ConstantDataView2D(TData value)
        {
            this.value = value;
        }

        public bool TryGet(int x, int y, out TData data)
        {
            data = value;
            return true;
        }

        public TData this[int x, int y] => value;
    }
}
