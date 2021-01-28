using System;

namespace RogueEntity.Core.Chunks
{
    public interface IDynamicViewChunkManagerData : IChunkManagerData
    {
        event EventHandler<DynamicViewChunkId> ViewChunkCreated;
        event EventHandler<DynamicViewChunkId> ViewChunkExpired;
        event EventHandler<DynamicViewChunkId> ViewChunkMarkedDirty;
        void RemoveViewChunk(DynamicViewChunkId chunkId);
    }
}
