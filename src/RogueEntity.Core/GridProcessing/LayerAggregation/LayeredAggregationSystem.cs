using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public class LayeredAggregationSystem<TGameContext, TAggregationType, TSourceType> : IAggregationLayerSystem<TGameContext, TAggregationType>,
                                                                                         IAggregationLayerSystemBackend<TGameContext, TSourceType>,
                                                                                         IReadOnlyDynamicDataView3D<TAggregationType>
    {
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;
        public event EventHandler<DynamicDataView3DEventArgs<TAggregationType>> ViewCreated;
        public event EventHandler<DynamicDataView3DEventArgs<TAggregationType>> ViewExpired;

        readonly AggregationLayerStore<TGameContext, TAggregationType, TSourceType> resultDataView;
        readonly List<IAggregationLayerController<TGameContext, TSourceType>> layerFactories2;
        readonly Action<AggregationProcessingParameter<TAggregationType, TSourceType>> aggregatorFunction;

        public LayeredAggregationSystem(Action<AggregationProcessingParameter<TAggregationType, TSourceType>> aggregator,
                                        int tileWidth,
                                        int tileHeight) : this(aggregator, 0, 0, tileWidth, tileHeight)
        {
        }

        public LayeredAggregationSystem([NotNull] Action<AggregationProcessingParameter<TAggregationType, TSourceType>> aggregator,
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
            this.resultDataView = new AggregationLayerStore<TGameContext, TAggregationType, TSourceType>();
            this.layerFactories2 = new List<IAggregationLayerController<TGameContext, TSourceType>>();
        }

        public int OffsetX { get; }
        public int OffsetY { get; }
        public int TileSizeX { get; }
        public int TileSizeY { get; }

        public void OnPositionDirty(object source, PositionDirtyEventArgs args)
        {
            if (resultDataView.MarkDirty(EntityGridPosition.From(args.Position)))
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

            foreach (var l in resultDataView.ZLayers)
            {
                resultDataView.RemoveLayer(l);
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

            foreach (var z in resultDataView.ZLayers)
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

            resultDataView.Process(context);
        }

        public bool TryGetView(int z, out IReadOnlyDynamicDataView2D<TAggregationType> data)
        {
            if (resultDataView.TryGetLayer(z, out var layerData))
            {
                data = layerData.ResistanceData;
                return true;
            }

            data = default;
            return false;
        }

        public DynamicDataViewConfiguration ViewConfiguration => this.ToConfiguration();

        public void Remove(int z)
        {
            resultDataView.RemoveLayer(z);
        }

        public bool TryGetSenseLayer(int z, out IAggregationPropertiesLayer<TGameContext, TSourceType> data)
        {
            if (resultDataView.TryGetLayer(z, out var dataImpl))
            {
                data = dataImpl;
                return true;
            }

            data = default;
            return false;
        }

        public IAggregationPropertiesLayer<TGameContext, TSourceType> GetOrCreate(int z)
        {
            if (resultDataView.TryGetLayer(z, out var dataImpl))
            {
                return dataImpl;
            }

            dataImpl = new AggregatePropertiesLayer<TGameContext, TAggregationType, TSourceType>(z, aggregatorFunction, OffsetX, OffsetY, TileSizeX, TileSizeY);
            resultDataView.DefineLayer(z, dataImpl);
            return dataImpl;
        }

        public void AddSenseLayerFactory(IAggregationLayerController<TGameContext, TSourceType> layerHandler)
        {
            layerFactories2.Add(layerHandler);
        }

        public IReadOnlyDynamicDataView3D<TAggregationType> ResultView => this;
    }
}