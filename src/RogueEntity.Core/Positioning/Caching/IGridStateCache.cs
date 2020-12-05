using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.Caching
{
    public interface IGridStateCache
    {
        bool IsDirty(int z, in Rectangle bounds);
        bool IsDirty<TPosition>(TPosition p) where TPosition: IPosition<TPosition>;
        bool IsDirty<TPosition>(TPosition p, float radius) where TPosition: IPosition<TPosition>;
    }
}