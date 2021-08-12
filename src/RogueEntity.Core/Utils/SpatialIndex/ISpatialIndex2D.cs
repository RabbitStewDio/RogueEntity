using System.Collections.Generic;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public interface ISpatialIndex2D<T>
    {
        int Insert(T data, in BoundingBox bounds);
        bool TryGet(int index, out T data, out BoundingBox boundingBox);
        void Remove(int elementIndex);

        List<int> Query(in BoundingBox bb, List<int> result, int skipElement = -1);
    }

    public static class SpatialIndexExtensions
    {

        public static List<int> Query<T>(this  ISpatialIndex2D<T> self, in BoundingBox bb, int skipElement = -1)
        {
            return self.Query(bb, new List<int>(), skipElement);
        }

    }
}
