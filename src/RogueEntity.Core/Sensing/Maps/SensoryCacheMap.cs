using System.Collections.Generic;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.MapChunks;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Maps
{
    public class SensoryCacheMap<TGameContext>: IReadOnlyMapData<SenseProperties>
    {
        readonly Dictionary<MapLayer, ISenseMapDataProcessor<TGameContext>> dependencies;
        readonly CombinedBlitterResultProcessor<TGameContext, ISenseMapDataProcessor<TGameContext>> combiner;
        readonly byte[] data;

        public int Height { get; }
        public int Width { get; }

        readonly int spanSizeX;
        readonly int spanSizeY;

        public SensoryCacheMap(IAddByteBlitter blitter, int width, int height)
        {
            (spanSizeX, spanSizeY) = ChunkProcessor.ComputeSpanSize(blitter, width, height);

            this.dependencies = new Dictionary<MapLayer, ISenseMapDataProcessor<TGameContext>>();
            data = new byte[width * height * 4];
            combiner = new CombinedBlitterResultProcessor<TGameContext, ISenseMapDataProcessor<TGameContext>>(width, height, spanSizeX, spanSizeY, data, blitter);

            Height = height;
            Width = width;
        }

        public void AddProcess(MapLayer layer, ISenseMapDataProcessor<TGameContext> p)
        {
            dependencies.Add(layer, p);
            combiner.Add(p);
        }

        public SenseProperties this[int x, int y]
        {
            get
            {
                var offset = (x + y * Width) * 4;
                return new SenseProperties(
                    Percentage.FromRaw(data[offset]),
                    Percentage.FromRaw(data[offset + 1]),
                    Percentage.FromRaw(data[offset + 2])
                    );

            }
        }


        public void MarkDirty(MapLayer l, EntityGridPosition pos)
        {
            if (l == MapLayer.Indeterminate)
            {
                foreach (var d in dependencies.Values)
                {
                    d.MarkDirty(pos.GridX, pos.GridY);
                }
            }
            else if (dependencies.TryGetValue(l, out var d))
            {
                d.MarkDirty(pos.GridX, pos.GridY);
            }
        }

        public void ResetDirtyFlags()
        {
            foreach (var d in dependencies.Values)
            {
                d.ResetDirtyFlags();
            }
        }

        public void Process(TGameContext c)
        {
            foreach (var d in dependencies.Values)
            {
                d.Process(c);
            }
            combiner.Process(c);
        }
    }
}