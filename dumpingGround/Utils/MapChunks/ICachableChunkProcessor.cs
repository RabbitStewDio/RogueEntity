namespace RogueEntity.Core.Utils.MapChunks
{
    public interface ICachableChunkProcessor<TContext> : IChunkProcessor<TContext>
    {
        void MarkDirty(int x, int y, int radius);
        void MarkDirty(int x, int y);
        void MarkClean(int x, int y);
        void ResetDirtyFlags();
    }
}