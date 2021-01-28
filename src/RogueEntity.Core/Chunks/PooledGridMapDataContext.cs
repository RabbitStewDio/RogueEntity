using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;
using System;

namespace RogueEntity.Core.Chunks
{
    public class PooledGridMapDataContext<TEntityId>: PooledDynamicDataView3D<TEntityId>, IGridMapDataContext<TEntityId>
    {
        public readonly MapLayer Layer;

        public PooledGridMapDataContext(MapLayer layer, IBoundedDataViewPool<TEntityId> pool) : base(pool)
        {
            this.Layer = layer;
        }

        public event EventHandler<PositionDirtyEventArgs> PositionDirty;

        public void MarkDirty<TPosition>(in TPosition position)
            where TPosition : IPosition<TPosition>
        {
            PositionDirty?.Invoke(this, new PositionDirtyEventArgs(Position.From(position)));
        }
    }
}
