using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public interface ISensePropertiesLayer<TGameContext, TSense>
    {
        bool IsDefined(MapLayer layer);
        void AddProcess(MapLayer layer, ISensePropertiesDataProcessor<TGameContext, TSense> p);
        void RemoveLayer(MapLayer layer);
    }
    
    public class SensePropertiesMap<TGameContext, TSense> : IReadOnlyDynamicDataView2D<SensoryResistance<TSense>>, 
                                                            ISensePropertiesLayer<TGameContext, TSense>
    {
        static readonly ILogger Logger = SLog.ForContext<SensePropertiesMap<TGameContext, TSense>>();
        
        readonly int z;
        readonly Dictionary<byte, ISensePropertiesDataProcessor<TGameContext, TSense>> dependencies;
        readonly DynamicDataView<SensoryResistance<TSense>> resistanceData;
        readonly List<ISensePropertiesDataProcessor<TGameContext, TSense>> dependenciesAsList;
        bool combinerDirty;

        public SensePropertiesMap(int z, int offsetX, int offsetY, int tileWidth, int tileHeight)
        {
            this.z = z;
            this.dependencies = new Dictionary<byte, ISensePropertiesDataProcessor<TGameContext, TSense>>();
            this.resistanceData = new DynamicDataView<SensoryResistance<TSense>>(offsetX, offsetY, tileWidth, tileHeight);
            this.dependenciesAsList = new List<ISensePropertiesDataProcessor<TGameContext, TSense>>();
        }

        public int TileSizeX
        {
            get { return resistanceData.TileSizeX; }
        }

        public int TileSizeY
        {
            get { return resistanceData.TileSizeY; }
        }

        public int OffsetX
        {
            get { return resistanceData.OffsetX; }
        }

        public int OffsetY
        {
            get { return resistanceData.OffsetY; }
        }

        public Rectangle GetActiveBounds()
        {
            return resistanceData.GetActiveBounds();
        }

        public List<Rectangle> GetActiveTiles(List<Rectangle> data = null)
        {
            return this.resistanceData.GetActiveTiles(data);
        }

        public bool IsDefined(MapLayer layer)
        {
            return dependencies.TryGetValue(layer.LayerId, out _);
        }

        public void AddProcess(MapLayer layer, ISensePropertiesDataProcessor<TGameContext, TSense> p)
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

        public bool TryGetData(int x, int y, out IReadOnlyBoundedDataView<SensoryResistance<TSense>> raw)
        {
            return resistanceData.TryGetData(x, y, out raw);
        }

        public bool TryGet(int x, int y, out SensoryResistance<TSense> result)
        {
            return resistanceData.TryGet(x, y, out result);
        }

        public SensoryResistance<TSense> this[int x, int y]
        {
            get
            {
                return resistanceData[x, y];
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
                foreach (var p in dependencies.Values)
                {
                    p.MarkDirty(x, y);
                }
            }
            else if (dependencies.TryGetValue(mapLayer, out var processorsByLayer))
            {
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
                var sp = new SensoryResistance<TSense>();
                foreach (var p in dependenciesAsList)
                {
                    if (p.Data.TryGet(x, y, out var d))
                    {
                        sp += d;
                    }
                }

                resistanceData.TrySet(x, y, sp);
            }
        }
    }
}