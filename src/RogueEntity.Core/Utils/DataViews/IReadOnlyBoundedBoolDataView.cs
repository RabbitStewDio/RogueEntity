namespace RogueEntity.Core.Utils.DataViews
{
    public interface IReadOnlyBoundedBoolDataView : IReadOnlyBoundedDataView<bool>
    {
        bool Any(int x, int y);
        bool AnyValueSet();
    }
}