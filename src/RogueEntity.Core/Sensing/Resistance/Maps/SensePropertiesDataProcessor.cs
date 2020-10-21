using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnTTSharp.Entities;
using GoRogue;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesDataProcessor<TGameContext, TItemId> : ISensePropertiesDataProcessor<TGameContext>
        where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TGameContext, TItemId>, IGridMapRawDataContext<TItemId>
        where TItemId : IEntityKey
    {

        readonly Action<(IDynamicDataView2D<TItemId> raw, TGameContext context, Rectangle bounds)> processFastDelegate;
        readonly Action<(IView2D<TItemId> raw, TGameContext context, Rectangle bounds)> processSlowDelegate;
        readonly int zPosition;
        readonly DynamicDataView<SensoryResistance> data;
        readonly DynamicBoolDataView dirtyMap;
        readonly List<Rectangle> activeTilesCache;
        readonly List<(IView2D<TItemId> raw, TGameContext context, Rectangle bounds)> processingSlowParameterCache;
        readonly List<(IDynamicDataView2D<TItemId> raw, TGameContext context, Rectangle bounds)> processingFastParameterCache;

        public SensePropertiesDataProcessor(MapLayer layer,
                                            int zPosition,
                                            int tw,
                                            int th)
        {
            this.Layer = layer;
            this.zPosition = zPosition;
            this.data = new DynamicDataView<SensoryResistance>(tw, th);
            this.dirtyMap = new DynamicBoolDataView(tw, th);
            this.activeTilesCache = new List<Rectangle>();
            this.processingFastParameterCache = new List<(IDynamicDataView2D<TItemId> raw, TGameContext context, Rectangle bounds)>();
            this.processingSlowParameterCache = new List<(IView2D<TItemId> raw, TGameContext context, Rectangle bounds)>();
            this.processFastDelegate = ProcessFast;
            this.processSlowDelegate = ProcessSlow;
        }

        public DynamicDataView<SensoryResistance> Data => data;

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

        public void Process(TGameContext context)
        {
            if (context.TryGetGridRawDataFor(Layer, out var mapDataRaw) &&
                mapDataRaw.TryGetRaw(zPosition, out var mapData))
            {
                ProcessRawData(context, mapData);
            }
            else if (context.TryGetGridDataFor(Layer, out var gridRaw) &&
                     gridRaw.TryGetMap(zPosition, out var gridMap))
            {
                ProcessData(context, gridMap);
            }
            
        }

        void ProcessData(TGameContext context, IView2D<TItemId> mapData)
        {
            var tc = dirtyMap.GetActiveTiles(activeTilesCache);
            processingSlowParameterCache.Clear();
            //
            // Remove all tiles that have not changed.
            for (var i = tc.Count - 1; i >= 0; i--)
            {
                var bounds = tc[i];
                if (dirtyMap.TryGetData(bounds.X, bounds.Y, out var dataTile))
                {
                    if (dataTile.AnyValueSet())
                    {
                        processingSlowParameterCache.Add((mapData, context, bounds));
                    }
                }
            }
            Parallel.ForEach(processingSlowParameterCache, processSlowDelegate);
        }
        
        void ProcessRawData(TGameContext context, IDynamicDataView2D<TItemId> mapData)
        {
            var tc = mapData.GetActiveTiles(activeTilesCache);
            processingFastParameterCache.Clear();
            //
            // Remove all tiles that have not changed.
            for (var i = tc.Count - 1; i >= 0; i--)
            {
                var bounds = tc[i];
                if (dirtyMap.TryGetData(bounds.X, bounds.Y, out var dataTile))
                {
                    if (dataTile.AnyValueSet())
                    {
                        processingFastParameterCache.Add((mapData, context, bounds));
                    }
                }
            }
            
            Parallel.ForEach(processingFastParameterCache, processFastDelegate);
        }
        
        void ProcessFast((IDynamicDataView2D<TItemId> raw, TGameContext context, Rectangle bounds) param)
        {
            var (raw, context, bounds) = param;
            if (!raw.TryGetData(bounds.MinExtentX, bounds.MinExtentY, out var groundData))
            {
                return;
            }

            var itemResolver = context.ItemResolver;
            foreach (var (x, y) in bounds)
            {
                var groundItemRef = groundData[x, y];
                if (itemResolver.TryQueryData(groundItemRef, context, out SensoryResistance groundItem))
                {
                    Data.TrySet(x, y, in groundItem);
                }
                else
                {
                    Data.TrySet(x, y, default);
                }
            }
        }
        
        void ProcessSlow((IView2D<TItemId> raw, TGameContext context, Rectangle bounds) param)
        {
            var (groundData, context, bounds) = param;
            var itemResolver = context.ItemResolver;
            foreach (var (x, y) in bounds)
            {
                var groundItemRef = groundData[x, y];
                if (itemResolver.TryQueryData(groundItemRef, context, out SensoryResistance groundItem))
                {
                    Data.TrySet(x, y, in groundItem);
                }
                else
                {
                    Data.TrySet(x, y, default);
                }
            }
        }
    }
}