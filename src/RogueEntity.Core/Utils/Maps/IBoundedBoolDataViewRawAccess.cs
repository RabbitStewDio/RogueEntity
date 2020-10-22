using GoRogue;

namespace RogueEntity.Core.Utils.Maps
{
    public interface IBoundedBoolDataViewRawAccess: IReadOnlyView2D<bool>
    {
        Rectangle Bounds { get; }
        byte[] Data { get; }
    }
}