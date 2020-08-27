using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Sensing.CacheControl
{
    public interface IMapCacheControl
    {
        void MarkDirty(EntityGridPosition p);
        void MarkDirty(ContinuousMapPosition p);
    }
}