using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Sensing.Cache
{
    public interface ISenseStateCacheProvider
    {
        public bool TryGetSenseCache<TSense>(out ISenseStateCacheView senseCache);
    }

    public interface IGlobalSenseStateCacheProvider
    {
        bool TryGetGlobalSenseCache(out ISenseStateCacheView senseCache);
    }

    public interface ISenseStateCacheControl
    {
        void MarkDirty<TSense>(in Position p);
    }
}