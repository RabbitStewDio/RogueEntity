namespace RogueEntity.Core.Sensing.Cache
{
    public interface ISenseStateCacheProvider
    {
        public bool TryGetSenseCache<TSense>(out ISenseStateCacheView senseCache);
    }
}