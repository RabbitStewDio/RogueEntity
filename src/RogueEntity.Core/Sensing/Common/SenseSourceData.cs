using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common
{
    /// <summary>
    ///   Stores the sense source data relative to a origin point. 
    /// </summary>
    public class SenseSourceData : IReadOnlyView2D<float>
    {
        public readonly byte[] Directions;
        public readonly float[] Intensities;
        public readonly int LineWidth;
        
        public SenseSourceData(int radius)
        {
            this.Radius = radius;
            LineWidth = 2 * radius + 1;
            Intensities = new float[LineWidth * LineWidth];
            Directions = new byte[LineWidth * LineWidth];
        }

        public int Radius { get; }

        public bool TryQuery(int x,
                             int y,
                             out float intensity,
                             out SenseDirection directionality,
                             out SenseDataFlags flags)
        {
            var dx = x + Radius;
            var dy = y + Radius;
            if (dx < 0 || dx >= LineWidth ||
                dy < 0 || dy >= LineWidth)
            {
                intensity = default;
                directionality = default;
                flags = default;
                return false;
            }
            
            var linIndex = dx + dy * LineWidth;
            intensity = Intensities[linIndex];
            var raw = new SenseDirectionStore(Directions[linIndex]);
            directionality = raw.Direction;
            flags = raw.Flags;
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
                          Position2D direction,
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

            Intensities[linIndex] = intensity;
            Directions[linIndex] = SenseDirectionStore.From(direction.X, direction.Y).With(flags).RawData;
        }

        public void Reset()
        {
            Array.Clear(Intensities, 0, Intensities.Length);
            Array.Clear(Directions, 0, Directions.Length);
        }
    }
}