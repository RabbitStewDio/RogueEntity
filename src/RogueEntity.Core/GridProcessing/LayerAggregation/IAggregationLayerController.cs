using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public interface IAggregationCacheControl
    {
        event EventHandler<PositionDirtyEventArgs> PositionDirty;
        void OnPositionDirty(object source, PositionDirtyEventArgs args);
    }

    public interface IAggregationPropertiesDataProcessor<TGameContext, TAggregateType>
    {
        MapLayer Layer { get; }
        int ZPosition { get; }
        DynamicDataView<TAggregateType> Data { get; }
        ReadOnlyListWrapper<Rectangle> ProcessedTiles { get; }

        void MarkDirty(int posGridX, int posGridY);
        void ResetDirtyFlags();
        bool Process(TGameContext context);
    }

    public interface IAggregationPropertiesLayer<TGameContext, TAggregateType>
    {
        bool IsDefined(MapLayer layer);
        void AddProcess(MapLayer layer, IAggregationPropertiesDataProcessor<TGameContext, TAggregateType> p);
        void RemoveLayer(MapLayer layer);
    }


    public interface IAggregationLayerSystem<TGameContext, TAggregateType> : IAggregationCacheControl
    {
        public IReadOnlyDynamicDataView3D<TAggregateType> ResultView { get; }
        public DynamicDataViewConfiguration ViewConfiguration { get; }

        void AddSenseLayerFactory(IAggregationLayerController<TGameContext, TAggregateType> layerHandler);

        bool TryGetSenseLayer(int z, out IAggregationPropertiesLayer<TGameContext, TAggregateType> data);
        IAggregationPropertiesLayer<TGameContext, TAggregateType> GetOrCreate(int z);
        void Remove(int z);
    }

    public interface IAggregationLayerController<TGameContext, TAggregateType>
    {
        void Start(TGameContext context, IAggregationLayerSystem<TGameContext, TAggregateType> system);
        void PrepareLayers(TGameContext ctx, IAggregationLayerSystem<TGameContext, TAggregateType> system);
        void Stop(TGameContext context, IAggregationLayerSystem<TGameContext, TAggregateType> system);
    }
}