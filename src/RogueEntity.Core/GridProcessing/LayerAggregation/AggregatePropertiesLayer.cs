using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public class AggregatePropertiesLayer< TAggregateData, TSourceType> : IAggregationPropertiesLayer< TSourceType>
    {
        static readonly ILogger Logger = SLog.ForContext<AggregatePropertiesLayer< TAggregateData, TSourceType>>();

        readonly int z;
        readonly Action<AggregationProcessingParameter<TAggregateData, TSourceType>> processor;
        readonly Dictionary<byte, IAggregationPropertiesDataProcessor< TSourceType>> dependencies;
        readonly DynamicDataView2D<TAggregateData> resistanceData;
        readonly List<IAggregationPropertiesDataProcessor< TSourceType>> dependenciesAsList;
        readonly List<IReadOnlyDynamicDataView2D<TSourceType>> dataViewsAsList;
        readonly Dictionary<Rectangle, AggregationProcessingParameter<TAggregateData, TSourceType>> processingParameterCollector;
        bool combinerDirty;

        public AggregatePropertiesLayer(int z,
                                        [NotNull] Action<AggregationProcessingParameter<TAggregateData, TSourceType>> processor,
                                        int offsetX,
                                        int offsetY,
                                        int tileSizeX,
                                        int tileSizeY)
        {
            this.z = z;
            this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
            this.dependencies = new Dictionary<byte, IAggregationPropertiesDataProcessor< TSourceType>>();
            this.resistanceData = new DynamicDataView2D<TAggregateData>(offsetX, offsetY, tileSizeX, tileSizeY);
            this.dependenciesAsList = new List<IAggregationPropertiesDataProcessor< TSourceType>>();
            this.dataViewsAsList = new List<IReadOnlyDynamicDataView2D<TSourceType>>();
            this.processingParameterCollector = new Dictionary<Rectangle, AggregationProcessingParameter<TAggregateData, TSourceType>>();
        }

        public DynamicDataView2D<TAggregateData> ResistanceData => resistanceData;

        public bool IsDefined(MapLayer layer)
        {
            return dependencies.TryGetValue(layer.LayerId, out _);
        }

        public void AddProcess(MapLayer layer, IAggregationPropertiesDataProcessor< TSourceType> p)
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


        public void Process()
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
                if (!d.Process())
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

                    proc = new AggregationProcessingParameter<TAggregateData, TSourceType>(t, dataViewsAsList, writableTile);
                    processingParameterCollector[t] = proc;
                }
            }

            Parallel.ForEach(processingParameterCollector.Values, processor);
            Logger.Verbose("Processed {ProcessedLayerCount} layers with {ProcessedTiles} tiles", processedLayers, processedLayerTiles);
        }
    }
}