using GoRogue;
using GoRogue.SenseMapping;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Vision
{
    public struct SenseSourceMap: IReadOnlyMapData<float>
    {
        ISenseSource senseSource;
        Coord lightMin;
        Coord lightMax;
        Coord globalMin;
        Coord globalMax;

        public SenseSourceMap(int width, int height): this()
        {
            Width = width;
            Height = height;
        }

        public void Initialize(ISenseSource senseSource)
        {
            if (senseSource == null)
            {
                this.senseSource = null;
                return;
            }

            this.senseSource = senseSource;
            var (lMin, lMax, gMin) = senseSource.CalculateBounds(Width, Height);
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