using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public abstract class GridAggregationPropertiesDataProcessor<TItemId, TAggregateType> : IAggregationPropertiesDataProcessor<TAggregateType>
        where TItemId : struct, IEntityKey
    {
        static readonly ILogger logger = SLog.ForContext<GridAggregationPropertiesDataProcessor<TItemId, TAggregateType>>();

        readonly Action<TileProcessingParameters> processFastDelegate;
        readonly IGridMapContext<TItemId> mapContext;
        readonly int zPosition;
        readonly DynamicBoolDataView dirtyMap;
        readonly BufferList<Rectangle> activeTilesCache;
        readonly List<TileProcessingParameters> processingFastParameterCache;
        readonly DynamicDataView2D<TAggregateType> writableDataView;
        bool dirtyAfterCreation;

        protected GridAggregationPropertiesDataProcessor(MapLayer layer,
                                                         IGridMapContext<TItemId> mapContext,
                                                         int zPosition,
                                                         int offsetX,
                                                         int offsetY,
                                                         int tileSizeX,
                                                         int tileSizeY)
        {
            this.Layer = layer;
            this.mapContext = mapContext ?? throw new ArgumentNullException(nameof(mapContext));
            this.zPosition = zPosition;
            this.writableDataView = new DynamicDataView2D<TAggregateType>(offsetX, offsetY, tileSizeX, tileSizeY);
            this.dirtyMap = new DynamicBoolDataView(offsetX, offsetY, tileSizeX, tileSizeY);
            this.activeTilesCache = new BufferList<Rectangle>();
            this.processingFastParameterCache = new List<TileProcessingParameters>();
            this.processFastDelegate = ProcessTile;
            this.dirtyAfterCreation = true;
        }

        public IReadOnlyDynamicDataView2D<TAggregateType> Data => writableDataView;
        public ReadOnlyListWrapper<Rectangle> ProcessedTiles => activeTilesCache;

        public int ZPosition => zPosition;
        public MapLayer Layer { get; }

        public void MarkDirty(int posGridX, int posGridY)
        {
            dirtyMap.TrySet(posGridX, posGridY, true);
        }

        public void ResetDirtyFlags()
        {
            dirtyMap.Clear();
        }

        public bool Process()
        {
            activeTilesCache.Clear();

            if (!mapContext.TryGetGridDataFor(Layer, out var mapDataRaw) ||
                !mapDataRaw.TryGetView(zPosition, out var mapData))
            {
                return false;
            }

            ProcessRawData(mapData);
            return true;
        }

        void ProcessRawData(IReadOnlyDynamicDataView2D<TItemId> mapData)
        {
            var tc = mapData.GetActiveTiles(activeTilesCache);
            processingFastParameterCache.Clear();
            //
            // Remove all tiles that have not changed.
            for (var i = tc.Count - 1; i >= 0; i--)
            {
                var bounds = tc[i];
                if (!dirtyAfterCreation && !dirtyMap.AnyValueSetInTile(bounds.X, bounds.Y))
                {
                    // skip processing if not marked as dirty.
                    continue;
                }

                if (mapData.TryGetData(bounds.X, bounds.Y, out var tile) &&
                    writableDataView.TryGetWriteAccess(bounds.X, bounds.Y, out var resultTile, DataViewCreateMode.CreateMissing))
                {
                    processingFastParameterCache.Add(new TileProcessingParameters(bounds, mapData, tile, resultTile));
                }
            }

            dirtyAfterCreation = false;

            if (processingFastParameterCache.Count == 0)
            {
                return;
            }

            logger.Verbose("Processing {Count} map tiles for z-Layer {ZLayer}", processingFastParameterCache.Count, zPosition);
            Parallel.ForEach(processingFastParameterCache, processFastDelegate);
        }

        protected readonly struct TileProcessingParameters
        {
            public readonly Rectangle Bounds;
            public readonly IReadOnlyDynamicDataView2D<TItemId> DataView;
            public readonly IReadOnlyBoundedDataView<TItemId> TileDataView;
            public readonly IBoundedDataView<TAggregateType> ResultTile;

            public TileProcessingParameters(Rectangle bounds,
                                            IReadOnlyDynamicDataView2D<TItemId> dataView,
                                            IReadOnlyBoundedDataView<TItemId> tileDataView,
                                            IBoundedDataView<TAggregateType> resultTile)
            {
                Bounds = bounds;
                DataView = dataView;
                TileDataView = tileDataView;
                ResultTile = resultTile;
            }

            public void Deconstruct(out Rectangle bounds,
                                    out IReadOnlyDynamicDataView2D<TItemId> dataView,
                                    out IReadOnlyBoundedDataView<TItemId> tileDataView,
                                    out IBoundedDataView<TAggregateType> resultTile)
            {
                bounds = Bounds;
                dataView = DataView;
                tileDataView = TileDataView;
                resultTile = ResultTile;
            }
        }

        protected abstract void ProcessTile(TileProcessingParameters p);
    }
}
