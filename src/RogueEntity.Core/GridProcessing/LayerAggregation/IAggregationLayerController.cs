using System;
using System.Diagnostics.CodeAnalysis;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public interface IAggregationCacheControl
    {
        event EventHandler<PositionDirtyEventArgs> PositionDirty;
    }

    public interface IAggregationPropertiesDataProcessor<TGameContext, TSourceType>
    {
        MapLayer Layer { get; }
        int ZPosition { get; }
        IReadOnlyDynamicDataView2D<TSourceType> Data { get; }
        ReadOnlyListWrapper<Rectangle> ProcessedTiles { get; }

        void MarkDirty(int posGridX, int posGridY);
        void ResetDirtyFlags();
        bool Process(TGameContext context);
    }

    public interface IAggregationPropertiesLayer<TGameContext, TSourceType>
    {
        bool IsDefined(MapLayer layer);
        void AddProcess(MapLayer layer, IAggregationPropertiesDataProcessor<TGameContext, TSourceType> p);
        void RemoveLayer(MapLayer layer);
    }


    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminator")]
    public interface IAggregationLayerSystem<TGameContext, TAggregateType> : IAggregationCacheControl
    { 
        public IReadOnlyDynamicDataView3D<TAggregateType> ResultView { get; }
    }

    public interface IAggregationLayerSystemBackend<TGameContext, TSourceType>
    {
        void OnPositionDirty(object source, PositionDirtyEventArgs args);

        public DynamicDataViewConfiguration ViewConfiguration { get; }

        void AddSenseLayerFactory(IAggregationLayerController<TGameContext, TSourceType> layerHandler);

        bool TryGetSenseLayer(int z, out IAggregationPropertiesLayer<TGameContext, TSourceType> data);
        IAggregationPropertiesLayer<TGameContext, TSourceType> GetOrCreate(int z);
        void Remove(int z);
    }
    
    public interface IAggregationLayerController<TGameContext, TSourceType>
    {
        void Start(TGameContext context, IAggregationLayerSystemBackend<TGameContext, TSourceType> system);
        void PrepareLayers(TGameContext ctx, IAggregationLayerSystemBackend<TGameContext, TSourceType> system);
        void Stop(TGameContext context, IAggregationLayerSystemBackend<TGameContext, TSourceType> system);
    }
}