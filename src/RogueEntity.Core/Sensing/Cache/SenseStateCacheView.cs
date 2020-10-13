using System;
using System.Collections.Generic;
using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Cache
{
    /// <summary>
    ///   A state change tracker that keeps track of changes to the game's map data. Changes
    ///   to the map usually also need to trigger a re-evaluation of sense data.  
    /// </summary>
    public class SenseStateCacheView : ISenseStateCacheView
    {
        readonly int resolution;
        readonly Dictionary<int, PackedBoolMap> trackersPerLayer;
        bool globallyDirty;

        public SenseStateCacheView(int resolution)
        {
            if (resolution < 1) throw new ArgumentException(nameof(resolution));
            
            this.resolution = resolution;
            this.trackersPerLayer = new Dictionary<int, PackedBoolMap>();
        }

        public void DeclareLayer(int z, int width, int height) 
        {
            var sizeX = (int)Math.Ceiling(width / (float)resolution);
            var sizeY = (int)Math.Ceiling(height / (float)resolution);
            trackersPerLayer[z] = new PackedBoolMap(sizeX, sizeY);
        }
        
        public void MarkClean()
        {
            foreach (var l in trackersPerLayer.Values)
            {
                l.Clear();
            }
            
            globallyDirty = false;
        }

        public void MarkDirty(Position p)
        {
            if (!trackersPerLayer.TryGetValue(p.GridZ, out var data))
            {
                return;
            }

            var px = p.GridX / resolution;
            var py = p.GridY / resolution;
            if (px < 0 || px >= data.Width)
            {
                return;
            }

            if (py < 0 || py >= data.Height)
            {
                return;
            }
                
            data[px, py] = true;
        }

        public bool IsDirty<TPosition>(TPosition p) where TPosition: IPosition
        {
            if (globallyDirty)
            {
                return true;
            }
            
            if (trackersPerLayer.TryGetValue(p.GridZ, out var data))
            {
                return data[p.GridX / resolution, p.GridY / resolution];
            }
            
            // always err on the safe side.
            return true;
        }

        public bool IsDirty<TPosition>(TPosition center, float radius) where TPosition: IPosition
        {
            if (globallyDirty)
            {
                return true;
            }

            if (!trackersPerLayer.TryGetValue(center.GridZ, out var map))
            {
                return false;
            }

            var radiusInt = (int)Math.Ceiling(radius);
            var rect = new Rectangle(new Coord(center.GridX, center.GridY), radiusInt, radiusInt);

            var minX = Math.Max(0, rect.MinExtentX / resolution);
            var minY = Math.Max(0, rect.MinExtentX / resolution);
            var maxX = Math.Min((int)Math.Ceiling(rect.MaxExtentX / (float)resolution), map.Width);
            var maxY = Math.Min((int)Math.Ceiling(rect.MaxExtentY / (float)resolution), map.Height);

            for (var y = minY; y < maxY; y += 1)
            {
                for (var x = minX; x < maxX; x += 1)
                {
                    if (map.Any(x, y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}