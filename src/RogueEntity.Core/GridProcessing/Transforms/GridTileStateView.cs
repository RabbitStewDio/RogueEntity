using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.Transforms
{
    public class GridTileStateView
    {
        readonly int offsetX;
        readonly int offsetY;
        readonly int tileSizeX;
        readonly int tileSizeY;
        readonly ConcurrentDictionary<int, DynamicBoolDataView> tileStateCache;
        readonly Func<int, DynamicBoolDataView> getActionDelegate;
        KeyValuePair<int, DynamicBoolDataView>[] copyBuffer;
        bool globallyDirty;

        public GridTileStateView(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.tileStateCache = new ConcurrentDictionary<int, DynamicBoolDataView>();
            this.globallyDirty = true;
            this.getActionDelegate = GetOrAddAction;
        }

        public void MarkGloballyDirty()
        {
            globallyDirty = true;
        }

        public void MarkDirty(in PositionDirtyEventArgs args)
        {
            MarkDirty(args.Position);
        }

        public void MarkDirty(in Position p)
        {
            var view = tileStateCache.GetOrAdd(p.GridZ, getActionDelegate);
            var dx = DataViewPartitions.TileSplit(p.GridX, offsetX, tileSizeX);
            var dy = DataViewPartitions.TileSplit(p.GridY, offsetY, tileSizeY);
            view[dx, dy] = true;
        }

        DynamicBoolDataView GetOrAddAction(int key)
        {
            return new DynamicBoolDataView(offsetX, offsetY, tileSizeX, tileSizeY);
        }

        /// <summary>
        ///   Should not be called from a multi-threaded context. Marking stuff dirty can
        ///   happen at any time, but marking stuff as clean should be its own tightly controlled
        /// </summary>
        public void MarkClean()
        {
            globallyDirty = false;
            ICollection<KeyValuePair<int, DynamicBoolDataView>> view = tileStateCache;
            var count = tileStateCache.Count;
            if (copyBuffer.Length < count)
            {
                Array.Resize(ref copyBuffer, tileStateCache.Count);
                view.CopyTo(copyBuffer, tileStateCache.Count);
            }

            for (var index = 0; index < count; index++)
            {
                var l = copyBuffer[index];
                l.Value.Clear();
            }
        }

        public bool IsDirty(in Position pos) => IsDirty(pos.GridX, pos.GridY, pos.GridZ);

        public bool IsDirty(int x, int y, int z)
        {
            if (globallyDirty)
            {
                return true;
            }

            if (!tileStateCache.TryGetValue(z, out var view))
            {
                return false;
            }

            var dx = DataViewPartitions.TileSplit(x, offsetX, tileSizeX);
            var dy = DataViewPartitions.TileSplit(y, offsetY, tileSizeY);
            return view[dx, dy];
        }
    }
}