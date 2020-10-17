namespace RogueEntity.Core.Sensing.Cache
{
    public interface ISenseStateCacheProvider
    {
        public bool TryGetSenseCache<TSense>(out ISenseStateCacheView senseCache);
    }

    public class NoOpSenseCacheProvider: ISenseStateCacheProvider
    {
        public bool TryGetSenseCache<TSense>(out ISenseStateCacheView senseCache)
        {
            senseCache = default;
            return false;
        }
    }
}