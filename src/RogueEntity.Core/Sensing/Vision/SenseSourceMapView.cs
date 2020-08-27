using System;
using GoRogue;
using GoRogue.SenseMapping;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Vision
{
    /// <summary>
    ///   Provides a simple way to access sense data. This class here encapsulates the necessary calculations
    ///   to provide a view over the sense source that can be accessed in global
    ///   coordinates.
    ///
    ///   SenseSources store their data in a local coordinate system that is centered around the origin of the
    ///   light source. This makes it inconvenient to quickly match sense data with actual map coordinates. 
    /// </summary>
    public struct SenseSourceMapView: IReadOnlyMapData<float>
    {
        ISenseSource senseSource;
        Coord lightMin;
        Coord lightMax;
        Coord globalMin;
        Coord globalMax;

        public SenseSourceMapView(int width, int height): this()
        {
            Width = width;
            Height = height;
        }

        public void Initialize(ISenseSource senseSourceParam)
        {
            this.senseSource = senseSourceParam ?? throw new ArgumentNullException(nameof(senseSourceParam));
            var (lMin, lMax, gMin) = senseSourceParam.CalculateBounds(Width, Height);
            this.lightMin = lMin;
            this.lightMax = lMax;
            this.globalMin = gMin;
            this.globalMax = gMin + (lightMax - lightMin);
        }

        public int Width { get; }
        public int Height { get; }

        public float this[int x, int y]
        {
            get
            {
                if (senseSource == null) return 1;
                if (x < globalMin.X || y < globalMin.Y) return 0;
                if (x > globalMax.X || y > globalMax.Y) return 0;

                var dx = x - globalMin.X;
                var dy = y - globalMin.Y;
                var lx = lightMin.X + dx;
                var ly = lightMin.Y + dy;
                return senseSource.SourceData[lx + ly * senseSource.Width];
            }
        }
    }
}