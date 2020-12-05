using System;
using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Positioning.Grid
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public interface IGridMapDataContext<TItemId>: IDynamicDataView3D<TItemId>
    {
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;
        void MarkDirty<TPosition>(in TPosition position) where TPosition: IPosition<TPosition>;
    }

    public interface IGridMapRawDataContext<TItemId>
    {
        bool TryGetRaw(int z, out IDynamicDataView2D<TItemId> data, MapAccess accessMode = MapAccess.ReadOnly);
    }

    public static class GridMapDataContextExtensions
    {
        public static bool TryGet<TItemId, TPosition>(this IGridMapDataContext<TItemId> data, in TPosition pos, out TItemId d)
            where TPosition: IPosition<TPosition>
        {
            if (!data.TryGetView(pos.GridZ, out var map))
            {
                d = default;
                return false;
            }

            return map.TryGet(pos.GridX, pos.GridY, out d);
        }

        public static bool TrySet<TItemId, TPosition>(this IGridMapDataContext<TItemId> data, in TPosition pos, in TItemId d)
            where TPosition: IPosition<TPosition>
        {
            if (!data.TryGetWritableView(pos.GridZ, out var map, DataViewCreateMode.CreateMissing))
            {
                return false;
            }

            return map.TrySet(pos.GridX, pos.GridY, in d);
        }
    }
}