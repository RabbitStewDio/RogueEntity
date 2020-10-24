using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoRogue;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using Serilog;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public interface ISensePropertiesLayer<TGameContext>
    {
        bool IsDefined(MapLayer layer);
        void AddProcess(MapLayer layer, ISensePropertiesDataProcessor<TGameContext> p);
        void RemoveLayer(MapLayer layer);
    }
    
    public class SensePropertiesMap<TGameContext> : IReadOnlyView2D<SensoryResistance>, ISensePropertiesLayer<TGameContext>
    {
        static readonly ILogger Logger = SLog.ForContext<SensePropertiesMap<TGameContext>>();
        
        readonly int z;
        readonly Dictionary<byte, ISensePropertiesDataProcessor<TGameContext>> dependencies;
        readonly DynamicDataView<SensoryResistance> data;
        readonly DynamicBoolDataView dataDirty;
        readonly List<ISensePropertiesDataProcessor<TGameContext>> dependenciesAsList;
        bool combinerDirty;

        public SensePropertiesMap(int z, int offsetX, int offsetY, int tileWidth, int tileHeight)
        {
            this.z = z;
            this.dependencies = new Dictionary<byte, ISensePropertiesDataProcessor<TGameContext>>();
            this.data = new DynamicDataView<SensoryResistance>(offsetX, offsetY, tileWidth, tileHeight);
            this.dataDirty = new DynamicBoolDataView(offsetX, offsetY, tileWidth, tileHeight);
            this.dependenciesAsList = new List<ISensePropertiesDataProcessor<TGameContext>>();
        }

        public bool IsDefined(MapLayer layer)
        {
            return dependencies.TryGetValue(layer.LayerId, out _);
        }

        public void AddProcess(MapLayer layer, ISensePropertiesDataProcessor<TGameContext> p)
        {
            dependencies.Add(layer.LayerId, p);
            combinerDirty = true;
        }

        public void RemoveLayer(MapLayer layer)
        {
            if (dependencies.TryGetValue(layer.LayerId, out _))
            {
                dependencies.Remove(layer.LayerId);
                combinerDirty = true;
            }
        }

        public bool TryGet(int x, int y, out SensoryResistance result)
        {
            return data.TryGet(x, y, out result);
        }

        public SensoryResistance this[int x, int y]
        {
            get
            {
                return data[x, y];
            }
        }

        public void MarkDirty(EntityGridPosition pos)
        {
            Console.WriteLine("DIRTY!" + pos);
            if (pos.IsInvalid || pos.GridZ != z)
            {
                return;
            }

            var x = pos.GridX;
            var y = pos.GridY;
            var mapLayer = pos.LayerId;
            if (mapLayer == MapLayer.Indeterminate.LayerId)
            {
                dataDirty.TrySet(x, y, true);
                foreach (var p in dependencies.Values)
                {
                    p.MarkDirty(x, y);
                }
            }
            else if (dependencies.TryGetValue(mapLayer, out var processorsByLayer))
            {
                dataDirty.TrySet(x, y, true);
                processorsByLayer.MarkDirty(x, y);
            }
        }

        public void ResetDirtyFlags()
        {
            foreach (var p in dependencies.Values)
            {
                p.ResetDirtyFlags();
            }
        }

        public void Process(TGameContext c)
        {
            var processedLayers = 0;
            var processedLayerTiles = 0;
            if (combinerDirty)
            {
                dependenciesAsList.Clear();
                foreach (var d in dependencies.Values)
                {
                    dependenciesAsList.Add(d);
                }

                combinerDirty = false;
            }
            
            var hs = new HashSet<Rectangle>();
            foreach (var d in dependenciesAsList)
            {
                if (!d.Process(c))
                {
                    continue;
                }

                processedLayers += 1;
                processedLayerTiles += d.ProcessedTiles.Count;
                foreach (var t in d.ProcessedTiles)
                {
                    hs.Add(t);
                }
            }

            Parallel.ForEach(hs, ProcessTile);
            Logger.Verbose("Processed {ProcessedLayerCount} layers with {ProcessedTiles} tiles", processedLayers, processedLayerTiles);
        }

        void ProcessTile(Rectangle bounds)
        {
            foreach (var (x, y) in bounds.Contents)
            {
                var sp = new SensoryResistance();
                foreach (var p in dependenciesAsList)
                {
                    if (p.Data.TryGet(x, y, out var d))
                    {
                        sp += d;
                    }
                }

                data.TrySet(x, y, sp);
            }
        }
    }
}