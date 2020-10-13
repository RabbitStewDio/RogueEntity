using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoRogue;
using GoRogue.MapViews;
using GoRogue.SenseMapping;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public interface IDirectionalSenseBlitterFactory
    {
        IDirectionalSenseBlitter Create();
    }
    
    public interface IDirectionalSenseBlitter
    {
        
    }

    public class DefaultDirectionalSenseSourceBlitter: IDirectionalSenseBlitter
    {
        
    }

    public readonly struct DirectionalSenseData
    {
        public readonly Direction Direction;
        public readonly float Intensity;

        public DirectionalSenseData(Direction direction, float intensity)
        {
            Direction = direction;
            Intensity = intensity;
        }
    }
    
    public class NoiseMap<TSenseSource>: IReadOnlyView2D<DirectionalSenseData> where TSenseSource : ISenseSource
    {
        readonly List<TSenseSource> senseSources;
        readonly IReadOnlyView2D<float> resistanceMap;

        readonly Action<TSenseSource> calculateLightParallelDelegate;
        readonly IDirectionalSenseBlitter blitterStrategy;
        
        float[] intensityMap;
        byte[] directionMap;

        /// <summary>
        /// Constructor. Takes the resistance map to use for calculations.
        /// </summary>
        /// <param name="resistanceMap">The resistance map to use for calculations.</param>
        /// <param name="blitter">A blitter.</param>
        public NoiseMap(IReadOnlyView2D<float> resistanceMap, 
                        IDirectionalSenseBlitterFactory blitter = null)
        {
            this.resistanceMap = resistanceMap;
            senseSources = new List<TSenseSource>();
            //calculateLightParallelDelegate = CalculateLightParallel;

            blitterStrategy = blitter?.Create() ?? new DefaultDirectionalSenseSourceBlitter();
        }

        public ReadOnlyListWrapper<TSenseSource> SenseSources => senseSources;

        /// <summary>
        /// Adds the given source to the list of sources. If the source has its
        /// <see cref="SenseSource.Enabled"/> flag set when <see cref="Calculate"/> is next called, then
        /// it will be counted as a source.
        /// </summary>
        /// <param name="senseSource">The source to add.</param>
        public void AddSenseSource(TSenseSource senseSource)
        {
            senseSources.Add(senseSource);
        }

        public Rectangle Bounds { get; private set; }

        public Rectangle ActiveBounds
        {
            get
            {
                if (senseSources.Count == 0) return new Rectangle();

                var minX = int.MaxValue;
                var minY = int.MaxValue;
                var maxX = int.MinValue;
                var maxY = int.MinValue;
                foreach (var senseSource in senseSources)
                {
                    var pos = senseSource.Position;
                    var rad = (int) Math.Ceiling(senseSource.Radius);

                    var gMin = pos - new Coord(rad, rad);
                    var gMax = pos + new Coord(rad, rad);
                    minX = Math.Min(minX, gMin.X);
                    minY = Math.Min(minY, gMin.Y);
                    maxX = Math.Max(maxX, gMax.X);
                    maxY = Math.Max(maxY, gMax.Y);
                }

                return new Rectangle(new Coord(minX, minY), new Coord(maxX, maxY));
            }
        }

        public void ClearSenseSources()
        {
            senseSources.Clear();
        }

        public DirectionalSenseData this[int x, int y]
        {
            get
            {
                // todo
                return default;
            }
        }
        
        /// <summary>
        /// Calcuates the map.  For each enabled source in the source list, it calculates
        /// the source's spreading, and puts them all together in the sense map's output.
        /// </summary>
        public void Calculate()
        {
            /*
            if (senseSources.Count > 16) // Probably not the proper condition, but useful for now.
            {
                Parallel.ForEach(senseSources, calculateLightParallelDelegate);
            }
            else
            {
                foreach (var senseSource in senseSources)
                {
                    senseSource.Calculate(resistanceMap);
                }
            }

            blitterStrategy.BeginBlit(senseMap, Width, Height);
            try
            {
                // todo:
                // This can be made parallel by subdividing the target
                // into non-overlapping tiles and then copying each 
                // tile in parallel.
                foreach (var senseSource in senseSources)
                {
                    blitterStrategy.BlitSenseSource(senseSource);
                }
            }
            finally
            {
                blitterStrategy.FinalizeBlit();
            }
            */
        }
    }
}