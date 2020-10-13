using System;
using System.Collections.Generic;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Positioning.Grid
{
    public interface IGridMapDataContext<TGameContext, TItemId>
    {
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;

        List<int> QueryActiveZLevels(List<int> cachedResults = null);
        bool TryGetMap(int z, out IMapData<TItemId> data, MapAccess accessMode = MapAccess.ReadOnly);
        void MarkDirty<TPosition>(in TPosition position) where TPosition: IPosition;
    }
}