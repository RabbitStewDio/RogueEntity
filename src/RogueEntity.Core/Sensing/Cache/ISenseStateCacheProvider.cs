using RogueEntity.Core.Positioning.Caching;

namespace RogueEntity.Core.Sensing.Cache
{
    public interface ISenseStateCacheProvider
    {
        public bool TryGetSenseCache<TSense>(out IGridStateCache senseCache);
    }

    public interface IGlobalSenseStateCacheProvider
    {
        bool TryGetGlobalSenseCache(out IGridStateCache senseCache);
    }
}