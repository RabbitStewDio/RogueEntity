using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Core.Positioning
{
    public interface IMapDataContext<TItemId> : IMapStateController
    {
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;
        public event EventHandler<MapRegionDirtyEventArgs> RegionDirty;

        public BufferList<int> GetActiveZLayers(BufferList<int>? buffer = null);
        public BufferList<Rectangle> GetActiveTiles(int z, BufferList<Rectangle>? buffer = null);
        
        public MapLayer Layer { get; }
        public bool AllowMultipleItems { get; }

        public BufferList<(TItemId, TPosition)> QueryItemArea<TPosition>(in Rectangle area, int z, BufferList<(TItemId, TPosition)>? buffer = null)
            where TPosition : struct, IPosition<TPosition>;
        
        public BufferList<(TItemId, TPosition)> QueryItemTile<TPosition>(in EntityGridPosition position, BufferList<(TItemId, TPosition)>? buffer = null)
            where TPosition : struct, IPosition<TPosition>;

        public BufferList<TItemId> QueryItem<TPosition>(in TPosition position, BufferList<TItemId>? buffer = null)
            where TPosition : struct, IPosition<TPosition>;

        public bool TryRemoveItem<TPosition>(TItemId itemId, in TPosition pos)
            where TPosition : struct, IPosition<TPosition>;

        public bool TryInsertItem<TPosition>(TItemId itemId, in TPosition desiredPosition)
            where TPosition : struct, IPosition<TPosition>;

        public bool TryUpdateItem<TPosition>(TItemId source, TItemId replacement, in TPosition desiredPosition)
            where TPosition : struct, IPosition<TPosition>;
    }
}