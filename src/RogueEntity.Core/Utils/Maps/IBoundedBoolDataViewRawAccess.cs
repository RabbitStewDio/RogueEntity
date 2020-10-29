namespace RogueEntity.Core.Utils.Maps
{
    public interface IBoundedBoolDataViewRawAccess: IBoundedDataView<bool>
    {
        byte[] Data { get; }
    }
}