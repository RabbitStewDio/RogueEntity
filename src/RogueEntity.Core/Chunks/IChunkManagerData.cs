using System;

namespace RogueEntity.Core.Chunks
{
    public interface IChunkManagerData : IDisposable
    {
        event EventHandler<int> ViewCreated;
        event EventHandler<int> ViewExpired;
        event EventHandler<int> ViewMarkedDirty;
        void RemoveView(int z);
    }
}
