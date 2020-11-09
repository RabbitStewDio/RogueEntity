using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public abstract class GridAggregationPropertiesDataProcessor<TGameContext, TItemId, TAggregateType> : IAggregationPropertiesDataProcessor<TGameContext, TAggregateType>
        where TItemId : IEntityKey
    {
        static readonly ILogger Logger = SLog.ForContext<GridAggregationPropertiesDataProcessor<TGameContext, TItemId, TAggregateType>>();

        readonly Action<TileProcessingParameters> processFastDelegate;
        readonly IGridMapContext<TItemId> mapContext;
        readonly int zPosition;
        readonly DynamicBoolDataView dirtyMap;
        readonly List<Rectangle> activeTilesCache;
        readonly List<TileProcessingParameters> processingFastParameterCache;
        readonly DynamicDataView<TAggregateType> writableDataView;
        bool dirtyAfterCreation;

        protected GridAggregationPropertiesDataProcessor(MapLayer layer,
                                                         [NotNull] IGridMapContext<TItemId> mapContext,
                                                         int zPosition,
                                                         int offsetX,
                                                         int offsetY,
                                                         int tileSizeX,
                                                         int tileSizeY)
        {
            this.Layer = layer;
            this.mapContext = mapContext ?? throw new ArgumentNullException(nameof(mapContext));
            this.zPosition = zPosition;
            this.writableDataView = new DynamicDataView<TAggregateType>(offsetX, offsetY, tileSizeX, tileSizeY);
            this.dirtyMap = new DynamicBoolDataView(offsetX, offsetY, tileSizeX, tileSizeY);
            this.activeTilesCache = new List<Rectangle>();
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
            dirtyMap.ClearData();
        }

        public bool Process(TGameContext context)
        {
            activeTilesCache.Clear();

            if (!mapContext.TryGetGridDataFor(Layer, out var mapDataRaw) ||
                !mapDataRaw.TryGetView(zPosition, out var mapData))
            {
                return false;
            }

            ProcessRawData(context, mapData);
            return true;
        }

        void ProcessRawData(TGameContext context, IReadOnlyDynamicDataView2D<TItemId> mapData)
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
                    processingFastParameterCache.Add(new TileProcessingParameters(bounds, context, mapData, tile, resultTile));
                }
            }

            dirtyAfterCreation = false;
            
            if (processingFastParameterCache.Count == 0)
            {
                return;
            }

            Logger.Verbose("Processing {Count} map tiles for z-Layer {ZLayer}", processingFastParameterCache.Count, zPosition);
            Parallel.ForEach(processingFastParameterCache, processFastDelegate);
        }

        protected readonly struct TileProcessingParameters
        {
            public readonly Rectangle Bounds;
            public readonly TGameContext Context;
            public readonly IReadOnlyDynamicDataView2D<TItemId> DataView;
            public readonly IReadOnlyBoundedDataView<TItemId> TileDataView;
            public readonly IBoundedDataView<TAggregateType> ResultTile;

            public TileProcessingParameters(Rectangle bounds,
                                            TGameContext context,
                                            IReadOnlyDynamicDataView2D<TItemId> dataView,
                                            IReadOnlyBoundedDataView<TItemId> tileDataView,
                                            IBoundedDataView<TAggregateType> resultTile)
            {
                Bounds = bounds;
                Context = context;
                DataView = dataView;
                TileDataView = tileDataView;
                ResultTile = resultTile;
            }

            public void Deconstruct(out Rectangle bounds,
                                    out TGameContext context,
                                    out IReadOnlyDynamicDataView2D<TItemId> dataView,
                                    out IReadOnlyBoundedDataView<TItemId> tileDataView,
                                    out IBoundedDataView<TAggregateType> resultTile)
            {
                bounds = Bounds;
                context = Context;
                dataView = DataView;
                tileDataView = TileDataView;
                resultTile = ResultTile;
            }
        }

        protected abstract void ProcessTile(TileProcessingParameters p);
    }
}