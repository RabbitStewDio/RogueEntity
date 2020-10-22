using System;
using JetBrains.Annotations;

namespace RogueEntity.Core.Sensing.Cache
{
    public class NoOpSenseCacheProvider: ISenseStateCacheProvider, IGlobalSenseStateCacheProvider
    {
        public bool TryGetSenseCache<TSense>(out ISenseStateCacheView senseCache)
        {
            senseCache = default;
            return false;
        }

        public bool TryGetGlobalSenseCache(out ISenseStateCacheView senseCache)
        {
            senseCache = default;
            return false;
        }
    }
}