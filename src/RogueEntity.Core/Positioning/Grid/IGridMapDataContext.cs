using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Positioning.Grid
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public interface IGridMapDataContext<TGameContext, TItemId>
    {
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;

        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
        List<int> QueryActiveZLevels(List<int> cachedResults = null);
        
        bool TryGetMap(int z, out IView2D<TItemId> data, MapAccess accessMode = MapAccess.ReadOnly);
        
        void MarkDirty<TPosition>(in TPosition position) where TPosition: IPosition;
    }

    public interface IGridMapRawDataContext<TItemId>
    {
        bool TryGetRaw(int z, out IDynamicDataView2D<TItemId> data, MapAccess accessMode = MapAccess.ReadOnly);
    }

    public static class GridMapDataContextExtensions
    {
        public static bool TryGet<TGameContext, TItemId, TPosition>(this IGridMapDataContext<TGameContext, TItemId> data, in TPosition pos, out TItemId d)
            where TPosition: IPosition
        {
            if (!data.TryGetMap(pos.GridZ, out var map))
            {
                d = default;
                return false;
            }

            return map.TryGet(pos.GridX, pos.GridY, out d);
        }

        public static bool TrySet<TGameContext, TItemId, TPosition>(this IGridMapDataContext<TGameContext, TItemId> data, in TPosition pos, in TItemId d)
            where TPosition: IPosition
        {
            if (!data.TryGetMap(pos.GridZ, out var map, MapAccess.ForWriting))
            {
                return false;
            }

            return map.TrySet(pos.GridX, pos.GridY, in d);
        }
    }
}