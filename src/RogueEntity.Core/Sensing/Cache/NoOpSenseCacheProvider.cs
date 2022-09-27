using RogueEntity.Core.Positioning.Caching;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Cache
{
    public class NoOpSenseCacheProvider: ISenseStateCacheProvider, IGlobalSenseStateCacheProvider
    {
        public bool TryGetSenseCache<TSense>([MaybeNullWhen(false)] out IGridStateCache senseCache)
        {
            senseCache = default;
            return false;
        }

        public bool TryGetGlobalSenseCache([MaybeNullWhen(false)] out IGridStateCache senseCache)
        {
            senseCache = default;
            return false;
        }
    }
}