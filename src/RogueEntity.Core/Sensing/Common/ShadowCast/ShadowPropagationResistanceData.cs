using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Common.ShadowCast
{
    public class ShadowPropagationResistanceData
    {
        float[] intensities;
        int lineWidth;

        public ShadowPropagationResistanceData(int radius)
        {
            this.Radius = radius;
            lineWidth = 2 * radius + 1;
            intensities = new float[lineWidth * lineWidth];
        }

        public int Radius { get; private set; }

        public Rectangle Bounds => new Rectangle(new Position2D(0, 0), Radius, Radius);

        public bool TryGet(int x, int y, out float intensity)
        {
            var dx = x + Radius;
            var dy = y + Radius;
            if (dx < 0 || dx >= lineWidth ||
                dy < 0 || dy >= lineWidth)
            {
                intensity = default;
                return false;
            }

            var linIndex = dx + dy * lineWidth;
            intensity = intensities[linIndex];
            return true;
        }

        public float this[int x, int y]
        {
            get
            {
                var dx = x + Radius;
                var dy = y + Radius;
                if (dx < 0 || dx >= lineWidth)
                {
                    return 1;
                }

                if (dy < 0 || dy >= lineWidth)
                {
                    return 1;
                }

                var linIndex = dx + dy * lineWidth;
                return intensities[linIndex];
            }
            set
            {
                var dx = x + Radius;
                var dy = y + Radius;
                if (dx < 0 || dx >= lineWidth)
                {
                    return;
                }

                if (dy < 0 || dy >= lineWidth)
                {
                    return;
                }

                var linIndex = dx + dy * lineWidth;
                intensities[linIndex] = value;
            }
        }

        public void Reset(int radius)
        {
            
            this.Radius = radius;
            lineWidth = 2 * radius + 1;
            if (intensities.Length < (lineWidth * lineWidth))
            {
                Array.Resize(ref intensities, lineWidth * lineWidth);
            }
            else
            {
                Array.Clear(intensities, 0, lineWidth * lineWidth);
            }
        }
    }
}