using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
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
        readonly IMapContext<TItemId> mapContext;
        readonly int zPosition;
        readonly DynamicBoolDataView2D dirtyMap;
        readonly BufferList<Rectangle> activeTilesCache;
        readonly List<TileProcessingParameters> processingFastParameterCache;
        readonly DynamicDataView2D<TAggregateType> writableDataView;
        bool dirtyAfterCreation;

        protected GridAggregationPropertiesDataProcessor(MapLayer layer,
                                                         IMapContext<TItemId> mapContext,
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
            this.dirtyMap = new DynamicBoolDataView2D(offsetX, offsetY, tileSizeX, tileSizeY);
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

            if (!mapContext.TryGetMapDataFor(Layer, out var mapData))
            {
                return false;
            }

            ProcessRawData(mapData);
            return true;
        }

        void ProcessRawData(IMapDataContext<TItemId> mapData)
        {
            var tc = mapData.GetActiveTiles(ZPosition, activeTilesCache);
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

                if (writableDataView.TryGetWriteAccess(bounds.X, bounds.Y, out var resultTile, DataViewCreateMode.CreateMissing))
                {
                    processingFastParameterCache.Add(new TileProcessingParameters(bounds, mapData, resultTile));
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
            public readonly IMapDataContext<TItemId> DataView;
            public readonly IBoundedDataView<TAggregateType> ResultTile;

            public TileProcessingParameters(Rectangle bounds,
                                            IMapDataContext<TItemId> dataView,
                                            IBoundedDataView<TAggregateType> resultTile)
            {
                Bounds = bounds;
                DataView = dataView;
                ResultTile = resultTile;
            }

            public void Deconstruct(out Rectangle bounds,
                                    out IMapDataContext<TItemId> dataView,
                                    out IBoundedDataView<TAggregateType> resultTile)
            {
                bounds = Bounds;
                dataView = DataView;
                resultTile = ResultTile;
            }
        }

        protected abstract void ProcessTile(TileProcessingParameters p);
    }
}
