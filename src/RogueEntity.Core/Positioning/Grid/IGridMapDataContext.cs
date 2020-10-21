using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Positioning.Grid
{
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
}