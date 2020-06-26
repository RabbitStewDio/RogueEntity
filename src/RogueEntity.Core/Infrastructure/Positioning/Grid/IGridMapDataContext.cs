using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Infrastructure.Positioning.Grid
{
    public interface IGridMapDataContext<TGameContext, TItemId>
    {
        bool TryGetMap(int z, out IMapData<TItemId> data);
        void MarkDirty(in EntityGridPosition position);
    }
}