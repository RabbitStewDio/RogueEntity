using RogueEntity.Core.Positioning.Caching;

namespace RogueEntity.Core.Sensing.Cache
{
    public class NoOpSenseCacheProvider: ISenseStateCacheProvider, IGlobalSenseStateCacheProvider
    {
        public bool TryGetSenseCache<TSense>(out IGridStateCache senseCache)
        {
            senseCache = default;
            return false;
        }

        public bool TryGetGlobalSenseCache(out IGridStateCache senseCache)
        {
            senseCache = default;
            return false;
        }
    }
}