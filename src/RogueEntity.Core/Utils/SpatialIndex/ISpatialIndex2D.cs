using System.Collections.Generic;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public interface ISpatialIndex2D<T>
    {
        FreeListIndex Insert(T data, in BoundingBox bounds);
        bool TryGet(FreeListIndex index, out T data, out BoundingBox boundingBox);
        void Remove(FreeListIndex elementIndex);

        List<FreeListIndex> Query(in BoundingBox bb, List<FreeListIndex> result, FreeListIndex skipElement = default);
    }

    public static class SpatialIndexExtensions
    {
        public static List<FreeListIndex> Query<T>(this  ISpatialIndex2D<T> self, in BoundingBox bb, FreeListIndex skipElement = default)
        {
            return self.Query(bb, new List<FreeListIndex>(), skipElement);
        }

    }
}
