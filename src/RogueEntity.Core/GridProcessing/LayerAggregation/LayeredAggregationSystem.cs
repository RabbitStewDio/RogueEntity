using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public readonly struct AggregationViewProcessedEvent<TAggregationType>
    {
        public readonly IAggregateDynamicDataView3D<TAggregationType> Source;
        public readonly IReadOnlyDynamicDataView2D<TAggregationType> LayerView;
        public readonly int ZInfo;
        public readonly IBoundedDataView<TAggregationType> Tile;

        public AggregationViewProcessedEvent(IAggregateDynamicDataView3D<TAggregationType> source, 
                                             int zInfo,
                                             IReadOnlyDynamicDataView2D<TAggregationType> layerView, 
                                             IBoundedDataView<TAggregationType> tile)
        {
            Source = source;
            LayerView = layerView;
            this.ZInfo = zInfo;
            Tile = tile;
        }
    }
    
    public interface IAggregateDynamicDataView3D<TAggregationType>: IReadOnlyDynamicDataView3D<TAggregationType>
    {
        public event EventHandler<AggregationViewProcessedEvent<TAggregationType>>? ViewProcessed;
    }
    
    public class LayeredAggregationSystem<TAggregationType, TSourceType> : IAggregationLayerSystem<TAggregationType>,
                                                                           IAggregationLayerSystemBackend<TSourceType>,
                                                                           IAggregateDynamicDataView3D<TAggregationType>
    {
#pragma warning disable CS0067 
        public event EventHandler<PositionDirtyEventArgs>? PositionDirty;
        public event EventHandler<DynamicDataView3DEventArgs<TAggregationType>>? ViewCreated;
        public event EventHandler<DynamicDataView3DEventArgs<TAggregationType>>? ViewReset;
        public event EventHandler<DynamicDataView3DEventArgs<TAggregationType>>? ViewExpired;
        public event EventHandler<AggregationViewProcessedEvent<TAggregationType>>? ViewProcessed;
#pragma warning restore CS0067 

        readonly AggregationLayerStore<TAggregationType, TSourceType> resultDataView;
        readonly List<IAggregationLayerController<TSourceType>> layerFactories2;
        readonly Action<AggregationProcessingParameter<TAggregationType, TSourceType>> aggregatorFunction;
        readonly List<AggregationViewProcessedEvent<TAggregationType>> processedViews;

        public LayeredAggregationSystem(Action<AggregationProcessingParameter<TAggregationType, TSourceType>> aggregator,
                                        int tileWidth,
                                        int tileHeight) : this(aggregator, 0, 0, tileWidth, tileHeight)
        { }

        public LayeredAggregationSystem(Action<AggregationProcessingParameter<TAggregationType, TSourceType>> aggregator,
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
            this.resultDataView = new AggregationLayerStore<TAggregationType, TSourceType>();
            this.resultDataView.ViewCreated += OnViewCreated;
            this.resultDataView.ViewExpired += OnViewExpired;
            this.layerFactories2 = new List<IAggregationLayerController<TSourceType>>();
            this.processedViews = new List<AggregationViewProcessedEvent<TAggregationType>>();
        }

        void OnViewExpired(object sender, DynamicDataView3DEventArgs<TAggregationType> e)
        {
            ViewExpired?.Invoke(this, e);
        }

        void OnViewCreated(object sender, DynamicDataView3DEventArgs<TAggregationType> e)
        {
            ViewCreated?.Invoke(this, e);
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

        public void Start()
        {
            foreach (var lf in layerFactories2)
            {
                lf.Start(this);
            }
        }

        public void Stop()
        {
            foreach (var lf in layerFactories2)
            {
                lf.Stop(this);
            }

            foreach (var l in resultDataView.ZLayers)
            {
                resultDataView.RemoveLayer(l);
            }
        }

        public BufferList<int> GetActiveLayers(BufferList<int>? buffer = null)
        {
            buffer = BufferList.PrepareBuffer(buffer);

            foreach (var z in resultDataView.ZLayers)
            {
                buffer.Add(z);
            }

            return buffer;
        }

        public void ProcessLayerData()
        {
            foreach (var lf in layerFactories2)
            {
                lf.PrepareLayers(this);
            }

            processedViews.Clear();
            resultDataView.Process(this, processedViews);

            for (var index = 0; index < processedViews.Count; index++)
            {
                var p = processedViews[index];
                ViewProcessed?.Invoke(this, p);
            }
        }

        public bool TryGetView(int z, [MaybeNullWhen(false)] out IReadOnlyDynamicDataView2D<TAggregationType> data)
        {
            if (resultDataView.TryGetLayer(z, out var layerData))
            {
                data = layerData.AggregatedView;
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

        public bool TryGetAggregationLayer(int z, [MaybeNullWhen(false)] out IAggregationPropertiesLayer<TSourceType> data)
        {
            if (resultDataView.TryGetLayer(z, out var dataImpl))
            {
                data = dataImpl;
                return true;
            }

            data = default;
            return false;
        }

        public IAggregationPropertiesLayer<TSourceType> GetOrCreate(int z)
        {
            if (resultDataView.TryGetLayer(z, out var dataImpl))
            {
                return dataImpl;
            }

            dataImpl = new AggregatePropertiesLayer<TAggregationType, TSourceType>(z, aggregatorFunction, OffsetX, OffsetY, TileSizeX, TileSizeY);
            resultDataView.DefineLayer(z, dataImpl);
            return dataImpl;
        }

        public void AddSenseLayerFactory(IAggregationLayerController<TSourceType> layerHandler)
        {
            layerFactories2.Add(layerHandler);
        }

        public IReadOnlyDynamicDataView3D<TAggregationType> ResultView => this;
    }
}
