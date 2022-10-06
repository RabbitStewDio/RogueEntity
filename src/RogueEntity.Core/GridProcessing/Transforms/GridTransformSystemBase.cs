using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RogueEntity.Core.GridProcessing.Transforms
{
    public abstract class GridTransformSystemBase<TSourceData, TTargetData>
    {
        protected abstract IReadOnlyDynamicDataView3D<TSourceData> SourceData { get; }
        protected abstract IDynamicDataView3D<TTargetData> TargetData { get; }
        
        readonly GridTileStateView dirtyMap;
        readonly List<ProcessingParameters> processingParameterCache;
        readonly BufferList<Rectangle> activeTileBuffer;
        readonly BufferList<int> activeZLevelsBuffer;
        readonly Action<ProcessingParameters> processTileDelegate;

        protected GridTransformSystemBase(DynamicDataViewConfiguration sourceData)
        {
            this.dirtyMap = new GridTileStateView(sourceData.OffsetX, sourceData.OffsetY, sourceData.TileSizeX, sourceData.TileSizeY);
            this.processingParameterCache = new List<ProcessingParameters>();
            this.activeTileBuffer = new BufferList<Rectangle>();
            this.activeZLevelsBuffer = new BufferList<int>();
            this.processTileDelegate = ProcessTile;
        }

        public bool Process()
        {
            processingParameterCache.Clear();

            var sourceData = SourceData;
            foreach (var zPosition in sourceData.GetActiveLayers(activeZLevelsBuffer))
            {
                if (!sourceData.TryGetView(zPosition, out var sourceView))
                {
                    continue;
                }

                // Check if this is a new layer. We'll first query without attempting to
                // create a missing layer to detect whether we are reusing an existing 
                // z-level. If that fails, we potentially have a new z-level we have not
                // seen before. In that case, our dirty map wont contain any data, so we
                // assume everything is dirty.
                bool maybeGloballyDirty = false;
                if (!TargetData.TryGetWritableView(zPosition, out var targetView))
                {
                    maybeGloballyDirty = true;
                    if (!TargetData.TryGetWritableView(zPosition, out targetView, DataViewCreateMode.CreateMissing))
                    {
                        continue;
                    }
                }
                
                foreach (var tile in sourceView.GetActiveTiles(activeTileBuffer))
                {
                    if (!maybeGloballyDirty && !dirtyMap.IsDirty(tile.X, tile.Y, zPosition))
                    {
                        continue;
                    }

                    if (!sourceView.TryGetData(tile.X, tile.Y, out var sourceTile) ||
                        !targetView.TryGetWriteAccess(tile.X, tile.Y, out var resultTile, DataViewCreateMode.CreateMissing))
                    {
                        continue;
                    }

                    processingParameterCache.Add(new ProcessingParameters(tile, zPosition, sourceView, sourceTile, targetView, resultTile));
                }
            }

            foreach (var z in TargetData.GetActiveLayers(activeZLevelsBuffer))
            {
                if (sourceData.TryGetView(z, out _))
                {
                    continue;
                }

                // This view is no longer contained in the source, so we can remove it from
                // here too.
                RemoveTargetDataLayer(z);
            }
            
            if (processingParameterCache.Count == 0)
            {
                return false;
            }
            
            var r = Parallel.ForEach(processingParameterCache, processTileDelegate);
            return r.IsCompleted;
        }

        protected abstract void RemoveTargetDataLayer(int z);
        
        public bool Process(Rectangle bounds)
        {
            processingParameterCache.Clear();

            var sourceData = SourceData;
            foreach (var zPosition in sourceData.GetActiveLayers(activeZLevelsBuffer))
            {
                if (!sourceData.TryGetView(zPosition, out var sourceView))
                {
                    continue;
                }

                if (!TargetData.TryGetWritableView(zPosition, out var targetView, DataViewCreateMode.CreateMissing))
                {
                    continue;
                }
                
                foreach (var tile in sourceView.GetActiveTiles(activeTileBuffer))
                {
                    if (!tile.Intersects(bounds))
                    {
                        continue;
                    }

                    if (!dirtyMap.IsDirty(tile.X, tile.Y, zPosition))
                    {
                        continue;
                    }

                    if (!sourceView.TryGetData(tile.X, tile.Y, out var sourceTile) ||
                        !targetView.TryGetWriteAccess(tile.X, tile.Y, out var resultTile, DataViewCreateMode.CreateMissing))
                    {
                        continue;
                    }

                    var effectiveArea = tile.GetIntersection(bounds);
                    processingParameterCache.Add(new ProcessingParameters(effectiveArea, zPosition, sourceView, sourceTile, targetView, resultTile));
                }
            }

            if (processingParameterCache.Count == 0)
            {
                return false;
            }
            
            var r = Parallel.ForEach(processingParameterCache, processTileDelegate);
            for (var i = 0; i < processingParameterCache.Count; i++)
            {
                var p = processingParameterCache[i];
                this.FireViewProcessedEvent(p.ZPosition, p.TargetLayer, p.TargetTile);
            }
            return r.IsCompleted;
        }

        protected abstract void FireViewProcessedEvent(int zPosition, 
                                                       IReadOnlyDynamicDataView2D<TTargetData> sourceLayer, 
                                                       IBoundedDataView<TTargetData> resultTile);

        protected abstract void ProcessTile(ProcessingParameters args);

        public void ExpireView(int z)
        {
            // todo: HERE
        }
        
        public void MarkGloballyDirty()
        {
            dirtyMap.MarkGloballyDirty();
        }

        public void MarkDirty(PositionDirtyEventArgs args)
        {
            dirtyMap.MarkDirty(args.Position);
        }

        public void MarkDirty(Position args)
        {
            dirtyMap.MarkDirty(args);
        }

        public void MarkClean()
        {
            dirtyMap.MarkClean();
        }

        public void MarkCleanSystem()
        {
            dirtyMap.MarkClean();
        }
        
        protected readonly struct ProcessingParameters
        {
            public readonly Rectangle Bounds;
            public readonly int ZPosition;
            public readonly IReadOnlyDynamicDataView2D<TSourceData> SourceLayer;
            public readonly IReadOnlyBoundedDataView<TSourceData> SourceTile;
            public readonly IDynamicDataView2D<TTargetData> TargetLayer;
            public readonly IBoundedDataView<TTargetData> TargetTile;

            public ProcessingParameters(Rectangle bounds,
                                        int zPosition,
                                        IReadOnlyDynamicDataView2D<TSourceData> sourceLayer,
                                        IReadOnlyBoundedDataView<TSourceData> sourceTile,
                                        IDynamicDataView2D<TTargetData> targetLayer,
                                        IBoundedDataView<TTargetData> targetTile)
            {
                Bounds = bounds;
                ZPosition = zPosition;
                SourceTile = sourceTile;
                this.TargetLayer = targetLayer;
                this.TargetTile = targetTile;
                SourceLayer = sourceLayer;
            }

            public void Deconstruct(out Rectangle bounds,
                                    out int zPosition,
                                    out IReadOnlyDynamicDataView2D<TSourceData> sourceLayer,
                                    out IReadOnlyBoundedDataView<TSourceData> sourceTile,
                                    out IBoundedDataView<TTargetData> resultTile)
            {
                sourceLayer = SourceLayer;
                bounds = Bounds;
                zPosition = ZPosition;
                sourceTile = SourceTile;
                resultTile = TargetTile;
            }
        }
    }
}
