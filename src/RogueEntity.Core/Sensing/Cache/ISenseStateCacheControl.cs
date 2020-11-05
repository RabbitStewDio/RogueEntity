using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Sensing.Cache
{
    public interface ISenseStateCacheControl
    {
        void MarkDirty<TSense>(in Position p);
    }
}