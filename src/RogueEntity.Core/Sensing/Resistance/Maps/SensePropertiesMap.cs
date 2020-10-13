using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.MapChunks;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesMap<TGameContext> : IReadOnlyMapData<SenseProperties>
    {
        readonly IAddByteBlitter blitter;
        readonly Dictionary<MapLayer, Dictionary<int, ISensePropertiesDataProcessor<TGameContext>>> dependencies;
        CombinedBlitterResultProcessor<TGameContext, ISensePropertiesDataProcessor<TGameContext>> combiner;
        byte[] data;
        bool combinerDirty;

        public int Height { get; private set; }
        public int Width { get; private set; }

        public SensePropertiesMap(IAddByteBlitter blitter, int width, int height)
        {
            this.blitter = blitter;
            this.dependencies = new Dictionary<MapLayer, Dictionary<int, ISensePropertiesDataProcessor<TGameContext>>>();

            data = new byte[0];
            Height = 0;
            Width = 0;
        }

        public bool IsDefined(MapLayer layer, int z)
        {
            return dependencies.TryGetValue(layer, out var x) && x.ContainsKey(z);
        }
        
        public void AddProcess(MapLayer layer, int z, ISensePropertiesDataProcessor<TGameContext> p)
        {
            if (!dependencies.TryGetValue(layer, out var processorByMapLayer))
            {
                processorByMapLayer = new Dictionary<int, ISensePropertiesDataProcessor<TGameContext>>();
                dependencies[layer] = processorByMapLayer;
            }

            if (processorByMapLayer.TryGetValue(z, out var dataProcessor))
            {
                throw new ArgumentException("Duplicate entry.");
            }

            processorByMapLayer.Add(z, p);
            combinerDirty = true;
        }

        public void RemoveLayer(MapLayer layer)
        {
            if (dependencies.TryGetValue(layer, out var processorByMapLayer))
            {
                processorByMapLayer.Clear();
                combinerDirty = true;
            }
        }
        
        public void RemoveProcess(MapLayer layer, int z)
        {
            if (!dependencies.TryGetValue(layer, out var processorByMapLayer))
            {
                return;
            }

            if (processorByMapLayer.Remove(z))
            {
                combinerDirty = true;
            }
        }
        
        public SenseProperties this[int x, int y]
        {
            get
            {
                if (combiner == null)
                {
                    return default;
                }
                
                var offset = (x + y * Width) * combiner.WordSize;
                return new SenseProperties(
                    Percentage.FromRaw(data[offset]),
                    Percentage.FromRaw(data[offset + 1]),
                    Percentage.FromRaw(data[offset + 2])
                );
            }
        }

        public void MarkDirty(MapLayer mapLayer, EntityGridPosition pos)
        {
            if (pos.IsInvalid)
            {
                return;
            }

            if (mapLayer == MapLayer.Indeterminate)
            {
                foreach (var procs in dependencies.Values)
                {
                    foreach (var d in procs.Values)
                    {
                        d.MarkDirty(pos.GridX, pos.GridY);
                    }
                }
            }
            else if (dependencies.TryGetValue(mapLayer, out var processorsByLayer) &&
                     processorsByLayer.TryGetValue(pos.GridZ, out var d))
            {
                d.MarkDirty(pos.GridX, pos.GridY);
            }
        }

        public void ResetDirtyFlags()
        {
            foreach (var procs in dependencies.Values)
            {
                foreach (var d in procs.Values)
                {
                    d.ResetDirtyFlags();
                }
            }
        }

        (Dimension size, int wordSize) ComputeMaxSize()
        {
            int width = 0;
            int height = 0;
            int wordSize = 0;
            foreach (var procs in dependencies.Values)
            {
                foreach (var d in procs.Values)
                {
                    width = Math.Max(d.Width, width);
                    height = Math.Max(d.Height, height);
                    wordSize = Math.Max(d.WordSize, wordSize);
                }
            }
            
            return (new Dimension(width, height), wordSize);
        }
        
        public void Process(TGameContext c)
        {
            EnsureCombinerIsValid();

            foreach (var procs in dependencies.Values)
            {
                foreach (var d in procs.Values)
                {
                    d.Process(c);
                }
            }

            combiner.Process(c);
        }

        void EnsureCombinerIsValid()
        {
            if (combinerDirty)
            {
                var (s, ws) = ComputeMaxSize();
                if (combiner == null ||
                    (combiner.Width < s.Width || combiner.Height < s.Height || ws != combiner.WordSize))
                {
                    Width = s.Width;
                    Height = s.Height;
                    var (spanSizeX, spanSizeY) = ChunkProcessor.ComputeSpanSize(blitter, s.Width, s.Height);
                    combiner = new CombinedBlitterResultProcessor<TGameContext, ISensePropertiesDataProcessor<TGameContext>>(
                        s.Width, s.Height, ws, spanSizeX, spanSizeY, data, blitter);
                    Array.Resize(ref data, Width * Height * ws);
                }

                combiner.Reset();
                foreach (var procs in dependencies.Values)
                {
                    foreach (var d in procs.Values)
                    {
                        combiner.Add(d);
                    }
                }
            }
        }
    }
}