using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common
{
    /// <summary>
    ///   Stores the sense source data relative to a origin point. 
    /// </summary>
    public class SenseSourceData : IReadOnlyView2D<float>
    {
        readonly SenseDirectionStore[] directions;
        readonly float[] intensities;
        readonly int lineWidth;
        
        public SenseSourceData(int radius)
        {
            this.Radius = radius;
            lineWidth = 2 * radius + 1;
            intensities = new float[lineWidth * lineWidth];
            directions = new SenseDirectionStore[lineWidth * lineWidth];
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
            if (dx < 0 || dx >= lineWidth ||
                dy < 0 || dy >= lineWidth)
            {
                intensity = default;
                directionality = default;
                flags = default;
                return false;
            }
            
            var linIndex = dx + dy * lineWidth;
            intensity = intensities[linIndex];
            var raw = directions[linIndex];
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
                if (dx < 0 || dx >= lineWidth)
                {
                    return default;
                }
                if (dy < 0 || dy >= lineWidth)
                {
                    return default;
                }
            
                var linIndex = dx + dy * lineWidth;
                return intensities[linIndex];
            }
        }

        public void Write(Position2D point,
                          float intensity, 
                          SenseDataFlags flags = SenseDataFlags.None)
        {
            var dx = point.X + Radius;
            var dy = point.Y + Radius;
            if (dx < 0 || dx >= lineWidth)
            {
                return;
            }
            if (dy < 0 || dy >= lineWidth)
            {
                return;
            }
            
            var linIndex = dx + dy * lineWidth;
            Console.WriteLine($"{dx}, {dy} = {intensity}");

            intensities[linIndex] = intensity;
            directions[linIndex] = SenseDirectionStore.From(point.X, point.Y).With(flags);
        }

        public void Reset()
        {
            Array.Clear(intensities, 0, intensities.Length);
        }
    }
    /// <summary>
    ///   Stores the sense source data relative to a origin point. 
    /// </summary>
    public class RippleSenseData : IReadOnlyView2D<bool>
    {
        public Queue<Position2D> OpenNodes { get; }
        public List<Position2D> NeighbourBuffer { get; }

        readonly bool[] backend;
        readonly int lineWidth;
        
        public RippleSenseData(int radius)
        {
            this.Radius = radius;
            this.OpenNodes = new Queue<Position2D>(256);
            this.NeighbourBuffer = new List<Position2D>(8);
            lineWidth = 2 * radius + 1;
            backend = new bool[lineWidth * lineWidth];
        }
        
        public int Radius { get; }
        
        public bool this[int x, int y]
        {
            get
            {
                var dx = x + Radius;
                var dy = y + Radius;
                return backend[dx + dy * lineWidth];
            }
            set
            {
                var dx = x + Radius;
                var dy = y + Radius;
                backend[dx + dy * lineWidth] = value;
            }
        }

        public void Reset()
        {
            Array.Clear(backend, 0, backend.Length);
        }
    }
}