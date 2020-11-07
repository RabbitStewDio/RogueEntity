using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesLayer<TGameContext, TSense> : ISensePropertiesLayer<TGameContext, TSense>
    {
        static readonly ILogger Logger = SLog.ForContext<SensePropertiesLayer<TGameContext, TSense>>();
        
        readonly int z;
        readonly Dictionary<byte, ISensePropertiesDataProcessor<TGameContext, TSense>> dependencies;
        readonly DynamicDataView<SensoryResistance<TSense>> resistanceData;
        readonly List<ISensePropertiesDataProcessor<TGameContext, TSense>> dependenciesAsList;
        readonly List<IReadOnlyDynamicDataView2D<SensoryResistance<TSense>>> dataViewsAsList;
        readonly Dictionary<Rectangle, AggregationProcessingParameter<SensoryResistance<TSense>>> processingParameterCollector;

        bool combinerDirty;

        public SensePropertiesLayer(int z, int offsetX, int offsetY, int tileWidth, int tileHeight)
        {
            this.z = z;
            this.dependencies = new Dictionary<byte, ISensePropertiesDataProcessor<TGameContext, TSense>>();
            this.resistanceData = new DynamicDataView<SensoryResistance<TSense>>(offsetX, offsetY, tileWidth, tileHeight);
            this.dependenciesAsList = new List<ISensePropertiesDataProcessor<TGameContext, TSense>>();
            this.dataViewsAsList = new List<IReadOnlyDynamicDataView2D<SensoryResistance<TSense>>>();
            this.processingParameterCollector = new Dictionary<Rectangle, AggregationProcessingParameter<SensoryResistance<TSense>>>();
        }

        public DynamicDataView<SensoryResistance<TSense>> ResistanceData => resistanceData;

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
                dataViewsAsList.Clear();
                foreach (var d in dependencies.Values)
                {
                    dataViewsAsList.Add(d.Data);
                    dependenciesAsList.Add(d);
                }

                combinerDirty = false;
            }

            processingParameterCollector.Clear();
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
                    if (processingParameterCollector.TryGetValue(t, out var proc))
                    {
                        continue;
                    }

                    if (!resistanceData.TryGetWriteAccess(t.X, t.Y, out var writableTile, DataViewCreateMode.CreateMissing))
                    {
                        continue;
                    }
                    
                    proc = new AggregationProcessingParameter<SensoryResistance<TSense>>(t, dataViewsAsList, writableTile);
                    processingParameterCollector[t] = proc;
                }
            }

            Parallel.ForEach(processingParameterCollector.Values, ProcessTile);
            Logger.Verbose("Processed {ProcessedLayerCount} layers with {ProcessedTiles} tiles", processedLayers, processedLayerTiles);
        }

        static void ProcessTile(AggregationProcessingParameter<SensoryResistance<TSense>> p)
        {
            var bounds = p.Bounds;
            var resistanceData = p.WritableTile;
            foreach (var (x, y) in bounds.Contents)
            {
                var sp = new SensoryResistance<TSense>();
                foreach (var dv in p.DataViews)
                {
                    if (dv.TryGet(x, y, out var d))
                    {
                        sp += d;
                    }
                }

                resistanceData.TrySet(x, y, sp);
            }
        }
    }
}