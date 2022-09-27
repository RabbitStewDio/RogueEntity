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

    public interface IAggregationPropertiesDataProcessor< TSourceType>
    {
        MapLayer Layer { get; }
        int ZPosition { get; }
        IReadOnlyDynamicDataView2D<TSourceType> Data { get; }
        ReadOnlyListWrapper<Rectangle> ProcessedTiles { get; }

        void MarkDirty(int posGridX, int posGridY);
        void ResetDirtyFlags();
        bool Process();
    }

    public interface IAggregationPropertiesLayer< TSourceType>
    {
        bool IsDefined(MapLayer layer);
        void AddProcess(MapLayer layer, IAggregationPropertiesDataProcessor< TSourceType> p);
        void RemoveLayer(MapLayer layer);
    }


    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminator")]
    public interface IAggregationLayerSystem<TAggregateType> : IAggregationCacheControl
    { 
        public IReadOnlyDynamicDataView3D<TAggregateType> ResultView { get; }
    }

    public interface IAggregationLayerSystemBackend< TSourceType>
    {
        void OnPositionDirty(object source, PositionDirtyEventArgs args);

        public DynamicDataViewConfiguration ViewConfiguration { get; }

        void AddSenseLayerFactory(IAggregationLayerController< TSourceType> layerHandler);

        bool TryGetSenseLayer(int z, [MaybeNullWhen(false)] out IAggregationPropertiesLayer< TSourceType> data);
        IAggregationPropertiesLayer< TSourceType> GetOrCreate(int z);
        void Remove(int z);
    }
    
    public interface IAggregationLayerController< TSourceType>
    {
        void Start(IAggregationLayerSystemBackend< TSourceType> system);
        void PrepareLayers(IAggregationLayerSystemBackend< TSourceType> system);
        void Stop(IAggregationLayerSystemBackend< TSourceType> system);
    }
}