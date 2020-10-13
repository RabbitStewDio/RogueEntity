using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Sensing.Cache
{
    public interface ISenseStateCacheView
    {
        bool IsDirty<TPosition>(TPosition p) where TPosition: IPosition;
        bool IsDirty<TPosition>(TPosition p, float radius) where TPosition: IPosition;
    }
}