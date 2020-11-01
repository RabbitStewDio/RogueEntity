using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common
{
    /// <summary>
    ///   Stores the sense source data relative to a origin point. 
    /// </summary>
    public sealed class SenseSourceData : IReadOnlyView2D<float>, ISenseDataView
    {
        public readonly byte[] Directions;
        public readonly float[] Intensities;
        public readonly int LineWidth;
        bool cleared;
        
        public SenseSourceData(int radius)
        {
            this.Radius = radius;
            LineWidth = 2 * radius + 1;
            Intensities = new float[LineWidth * LineWidth];
            Directions = new byte[LineWidth * LineWidth];
        }

        public int Radius { get; }

        public Rectangle Bounds => new Rectangle(new Position2D(0,0), Radius, Radius);
        
        public bool TryQuery(int x,
                             int y,
                             out float intensity,
                             out SenseDirectionStore directionality)
        {
            var dx = x + Radius;
            var dy = y + Radius;
            if (dx < 0 || dx >= LineWidth ||
                dy < 0 || dy >= LineWidth)
            {
                intensity = default;
                directionality = default;
                return false;
            }
            
            var linIndex = dx + dy * LineWidth;
            intensity = Intensities[linIndex];
            directionality = new SenseDirectionStore(Directions[linIndex]);
            return true;
        }

        public bool TryGet(int x, int y, out float intensity)
        {
            var dx = x + Radius;
            var dy = y + Radius;
            if (dx < 0 || dx >= LineWidth ||
                dy < 0 || dy >= LineWidth)
            {
                intensity = default;
                return false;
            }
            
            var linIndex = dx + dy * LineWidth;
            intensity = Intensities[linIndex];
            return true;
        }

        public float this[int x, int y]
        {
            get
            {
                var dx = x + Radius;
                var dy = y + Radius;
                if (dx < 0 || dx >= LineWidth)
                {
                    return default;
                }
                if (dy < 0 || dy >= LineWidth)
                {
                    return default;
                }
            
                var linIndex = dx + dy * LineWidth;
                return Intensities[linIndex];
            }
        }

        public void Write(Position2D point,
                          float intensity, 
                          SenseDirection direction = SenseDirection.None, 
                          SenseDataFlags flags = SenseDataFlags.None)
        {
            var dx = point.X + Radius;
            var dy = point.Y + Radius;
            if (dx < 0 || dx >= LineWidth)
            {
                return;
            }
            if (dy < 0 || dy >= LineWidth)
            {
                return;
            }

            var linIndex = dx + dy * LineWidth;
            Intensities[linIndex] = intensity;
            Directions[linIndex] = SenseDirectionStore.From(direction, flags).RawData;
        }

        public void Write(Position2D point,
                          Position2D origin,
                          float intensity, 
                          SenseDataFlags flags = SenseDataFlags.None)
        {
            var dx = point.X + Radius;
            var dy = point.Y + Radius;
            if (dx < 0 || dx >= LineWidth)
            {
                return;
            }
            if (dy < 0 || dy >= LineWidth)
            {
                return;
            }
            
            var linIndex = dx + dy * LineWidth;

            var direction = point - origin;
            Intensities[linIndex] = intensity;
            Directions[linIndex] = SenseDirectionStore.From(direction.X, direction.Y).With(flags).RawData;
        }

        
        public void Reset()
        {
            if (!cleared)
            {
                Array.Clear(Intensities, 0, Intensities.Length);
                Array.Clear(Directions, 0, Directions.Length);
                cleared = true;
            }
        }

        public void MarkWritten()
        {
            cleared = false;
        }
    }
}