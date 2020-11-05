namespace RogueEntity.Core.Utils.DataViews
{
    public interface IBoundedBoolDataViewRawAccess: IBoundedDataView<bool>
    {
        byte[] Data { get; }
    }
}