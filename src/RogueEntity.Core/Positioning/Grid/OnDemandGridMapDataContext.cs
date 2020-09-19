using System.Collections.Generic;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Positioning.Grid
{
    public class OnDemandGridMapDataContext<TGameContext, TItemId> : IGridMapDataContext<TGameContext, TItemId>
    {
        public int Width { get; }
        public int Height { get; }
        readonly Dictionary<int, IMapData<TItemId>> mapDataByDepth;

        public OnDemandGridMapDataContext(int width, int height)
        {
            Width = width;
            Height = height;
            mapDataByDepth = new Dictionary<int, IMapData<TItemId>>();
        }

        public bool TryGetMap(int z, out IMapData<TItemId> data)
        {
            if (mapDataByDepth.TryGetValue(z, out data))
            {
                return true;
            }

            data = new DenseMapData<TItemId>(Width, Height);
            mapDataByDepth[z] = data;
            return true;
        }

        public void MarkDirty(in EntityGridPosition position)
        {
            // not needed here.
        }
    }
}