using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.SpatialIndex
{
    public interface ISpatialIndex2D<T>
    {
        int ElementIndexRange { get; }

        FreeListIndex Insert(T data, in BoundingBox bounds);
        bool TryGet(FreeListIndex index, [MaybeNullWhen(false)] out T data, out BoundingBox boundingBox);
        void Remove(FreeListIndex elementIndex);
        
        BoundingBox Bounds { get; }
        
        BufferList<FreeListIndex> QueryIndex(in BoundingBox bb, 
                                        BufferList<FreeListIndex>? result = null, 
                                        FreeListIndex skipElement = default);

        BufferList<T> Query(in BoundingBox bb,
                            BufferList<T>? result = default,
                            FreeListIndex skipElement = default);

        void Clear();
    }
}
