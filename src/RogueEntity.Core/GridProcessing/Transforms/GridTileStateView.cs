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
        readonly Dictionary<int, DynamicBoolDataView2D> tileStateCache;
        bool globallyDirty;

        public GridTileStateView(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.tileStateCache = new Dictionary<int, DynamicBoolDataView2D>();
            this.globallyDirty = true;
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
            if (!tileStateCache.TryGetValue(p.GridZ, out var view))
            {
                view = new DynamicBoolDataView2D(offsetX, offsetY, tileSizeX, tileSizeY);
                tileStateCache[p.GridZ] = view;
            }
            
            var dx = DataViewPartitions.TileSplit(p.GridX, offsetX, tileSizeX);
            var dy = DataViewPartitions.TileSplit(p.GridY, offsetY, tileSizeY);
            view[dx, dy] = true;
        }

        /// <summary>
        ///   Should not be called from a multi-threaded context. Marking stuff dirty can
        ///   happen at any time, but marking stuff as clean should be its own tightly controlled
        /// </summary>
        public void MarkClean()
        {
            globallyDirty = false;
            foreach (var l in tileStateCache.Values)
            {
                l.Clear();
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