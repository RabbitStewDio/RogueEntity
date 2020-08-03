using RogueEntity.Core.Infrastructure.Positioning.Continuous;
using RogueEntity.Core.Infrastructure.Positioning.Grid;

namespace RogueEntity.Core.Infrastructure
{
    public interface IMapCacheControlProvider
    {
        IMapCacheControl MapCacheControl { get; }
    }

    public interface IMapCacheControl
    {
        void MarkDirty(EntityGridPosition p);
        void MarkDirty(ContinuousMapPosition p);
    }
}