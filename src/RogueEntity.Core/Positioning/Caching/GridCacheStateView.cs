using System;
using System.Collections.Generic;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Positioning.Caching
{
    /// <summary>
    ///   A state change tracker that keeps track of changes to the game's map data. Changes
    ///   to the map usually also need to trigger a re-evaluation of sense data.  
    /// </summary>
    public class GridCacheStateView : IGridStateCache
    {
        readonly int offsetX;
        readonly int offsetY;
        readonly int tileSizeX;
        readonly int tileSizeY;
        readonly int resolution;
        readonly Dictionary<int, DynamicBoolDataView2D> trackersPerLayer;
        bool globallyDirty;

        public GridCacheStateView(int resolution, int tileSizeX, int tileSizeY) : this(resolution, 0, 0, tileSizeX, tileSizeY)
        {
        }

        public GridCacheStateView(int resolution, int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            if (resolution < 1) throw new ArgumentException(nameof(resolution));

            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.resolution = resolution;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.trackersPerLayer = new Dictionary<int, DynamicBoolDataView2D>();
        }

        public void MarkGloballyDirty()
        {
            globallyDirty = true;
        }
        
        public void MarkClean()
        {
            foreach (var l in trackersPerLayer.Values)
            {
                l.Clear();
            }

            globallyDirty = false;
        }

        public void MarkDirty(in Position p)
        {
            if (!trackersPerLayer.TryGetValue(p.GridZ, out var data))
            {
                data = new DynamicBoolDataView2D(offsetX, offsetY, tileSizeX, tileSizeY);
                trackersPerLayer[p.GridZ] = data;
            }

            var px = p.GridX / resolution;
            var py = p.GridY / resolution;
            var view = data.GetOrCreateData(px, py);
            view[px, py] = true;
        }

        public bool IsTileDirty(int z, GridPosition2D pos)
        {
            if (globallyDirty)
            {
                return true;
            }

            if (!trackersPerLayer.TryGetValue(z, out var map))
            {
                return false;
            }

            if (!map.TryGetData(pos.X, pos.Y, out var data))
            {
                // cannot be dirty
                return false;
            }

            if (!data.AnyValueSet())
            {
                return false;
            }

            return true;
        }
        
        public bool IsDirty<TPosition>(TPosition p)
            where TPosition : IPosition<TPosition>
        {
            if (globallyDirty)
            {
                return true;
            }

            if (trackersPerLayer.TryGetValue(p.GridZ, out var data))
            {
                return data[p.GridX / resolution, p.GridY / resolution];
            }

            return false;
        }

        public bool IsDirty(int z, in Rectangle rect)
        {
            if (globallyDirty)
            {
                return true;
            }

            if (!trackersPerLayer.TryGetValue(z, out var map))
            {
                return false;
            }

            var minX = Math.Max(0, rect.MinExtentX / resolution);
            var minY = Math.Max(0, rect.MinExtentX / resolution);
            var maxX = (int)Math.Ceiling(rect.MaxExtentX / (float)resolution);
            var maxY = (int)Math.Ceiling(rect.MaxExtentY / (float)resolution);
            var scaledBounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            
            for (var y = minY; y < maxY; y += tileSizeY)
            {
                for (var x = minX; x < maxX; x += tileSizeX)
                {
                    if (!map.TryGetData(x, y, out var data))
                    {
                        // cannot be dirty
                        continue;
                    }

                    if (!data.AnyValueSet())
                    {
                        continue;
                    }

                    var limit = data.Bounds.GetIntersection(in scaledBounds);
                    foreach (var lpos in limit.Contents)
                    {
                        if (map[lpos.X, lpos.Y])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool IsDirty<TPosition>(TPosition center, float radius)
            where TPosition : IPosition<TPosition>
        {
            if (globallyDirty)
            {
                return true;
            }

            var radiusInt = (int)Math.Ceiling(radius);
            var rect = new Rectangle(new GridPosition2D(center.GridX, center.GridY), radiusInt, radiusInt);
            return IsDirty(center.GridZ, in rect);
        }
    }
}