using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Infrastructure.Positioning.Grid
{
    public interface IGridMapDataContext<TGameContext, TItemId>
    {
        int Width { get; }
        int Height { get; }
        
        bool TryGetMap(int z, out IMapData<TItemId> data);
        void MarkDirty(in EntityGridPosition position);
    }
}