using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public class LayeredAggregationSystem<TGameContext, TAggregationType> : IAggregationLayerSystem<TGameContext, TAggregationType>, 
                                                                            IReadOnlyDynamicDataView3D<TAggregationType>
    {
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;

        readonly AggregationLayerStore<TGameContext, TAggregationType> propertiesMap;
        readonly List<IAggregationLayerController<TGameContext, TAggregationType>> layerFactories2;
        readonly Action<AggregationProcessingParameter<TAggregationType>> aggregatorFunction;

        public LayeredAggregationSystem(Action<AggregationProcessingParameter<TAggregationType>> aggregator,
                                        int tileWidth,
                                        int tileHeight) : this(aggregator, 0, 0, tileWidth, tileHeight)
        {
        }

        public LayeredAggregationSystem([NotNull] Action<AggregationProcessingParameter<TAggregationType>> aggregator,
                                        int offsetX,
                                        int offsetY,
                                        int tileSizeX,
                                        int tileSizeY)
        {
            this.aggregatorFunction = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.TileSizeX = tileSizeX;
            this.TileSizeY = tileSizeY;
            this.propertiesMap = new AggregationLayerStore<TGameContext, TAggregationType>();
            this.layerFactories2 = new List<IAggregationLayerController<TGameContext, TAggregationType>>();
        }

        public int OffsetX { get; }
        public int OffsetY { get; }
        public int TileSizeX { get; }
        public int TileSizeY { get; }

        public void OnPositionDirty(object source, PositionDirtyEventArgs args)
        {
            if (propertiesMap.MarkDirty(EntityGridPosition.From(args.Position)))
            {
                PositionDirty?.Invoke(this, args);
            }
        }

        public void Start(TGameContext context)
        {
            foreach (var lf in layerFactories2)
            {
                lf.Start(context, this);
            }
        }

        public void Stop(TGameContext context)
        {
            foreach (var lf in layerFactories2)
            {
                lf.Stop(context, this);
            }

            foreach (var l in propertiesMap.ZLayers)
            {
                propertiesMap.RemoveLayer(l);
            }
        }

        public List<int> GetActiveLayers(List<int> buffer = null)
        {
            if (buffer == null)
            {
                buffer = new List<int>();
            }
            else
            {
                buffer.Clear();
            }

            foreach (var z in propertiesMap.ZLayers)
            {
                buffer.Add(z);
            }

            return buffer;
        }

        public void ProcessSenseProperties(TGameContext context)
        {
            foreach (var lf in layerFactories2)
            {
                lf.PrepareLayers(context, this);
            }

            propertiesMap.Process(context);
        }

        public bool TryGetView(int z, out IReadOnlyDynamicDataView2D<TAggregationType> data)
        {
            if (propertiesMap.TryGetLayer(z, out var layerData))
            {
                data = layerData.ResistanceData;
                return true;
            }

            data = default;
            return false;
        }

        public DynamicDataViewConfiguration ViewConfiguration => this.ToConfiguration();
        
        public IReadOnlyDynamicDataView3D<TAggregationType> ResultView => this;

        public void Remove(int z)
        {
            propertiesMap.RemoveLayer(z);
        }

        public bool TryGetSenseLayer(int z, out IAggregationPropertiesLayer<TGameContext, TAggregationType> data)
        {
            if (propertiesMap.TryGetLayer(z, out var dataImpl))
            {
                data = dataImpl;
                return true;
            }

            data = default;
            return false;
        }

        public IAggregationPropertiesLayer<TGameContext, TAggregationType> GetOrCreate(int z)
        {
            if (propertiesMap.TryGetLayer(z, out var dataImpl))
            {
                return dataImpl;
            }

            dataImpl = new AggregatePropertiesLayer<TGameContext, TAggregationType>(z, OffsetX, OffsetY, TileSizeX, TileSizeY, aggregatorFunction);
            propertiesMap.DefineLayer(z, dataImpl);
            return dataImpl;
        }

        public void AddSenseLayerFactory(IAggregationLayerController<TGameContext, TAggregationType> layerHandler)
        {
            layerFactories2.Add(layerHandler);
        }
    }
}