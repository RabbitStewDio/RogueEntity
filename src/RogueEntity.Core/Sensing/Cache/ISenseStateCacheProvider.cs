using RogueEntity.Core.Positioning.Caching;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Sensing.Cache
{
    public interface ISenseStateCacheProvider
    {
        public bool TryGetSenseCache<TSense>([MaybeNullWhen(false)] out IGridStateCache senseCache);
    }

    public interface IGlobalSenseStateCacheProvider
    {
        bool TryGetGlobalSenseCache([MaybeNullWhen(false)] out IGridStateCache senseCache);
    }
}