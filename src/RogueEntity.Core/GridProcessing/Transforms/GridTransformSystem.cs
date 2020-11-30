using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.Transforms
{
    public abstract class GridTransformSystemBase<TSourceData, TTargetData>
    {
        protected abstract IReadOnlyDynamicDataView3D<TSourceData> SourceData { get; }
        protected abstract IDynamicDataView3D<TTargetData> TargetData { get; }
        
        readonly GridTileStateView dirtyMap;
        readonly List<ProcessingParameters> processingParameterCache;
        readonly List<Rectangle> activeTileBuffer;
        readonly List<int> activeZLevelsBuffer;
        readonly Action<ProcessingParameters> processTileDelegate;

        protected GridTransformSystemBase(DynamicDataViewConfiguration sourceData)
        {
            this.dirtyMap = new GridTileStateView(sourceData.OffsetX, sourceData.OffsetY, sourceData.TileSizeX, sourceData.TileSizeY);
            this.processingParameterCache = new List<ProcessingParameters>();
            this.activeTileBuffer = new List<Rectangle>();
            this.activeZLevelsBuffer = new List<int>();
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

                if (!TargetData.TryGetWritableView(zPosition, out var directionMap, DataViewCreateMode.CreateMissing))
                {
                    continue;
                }
                
                foreach (var tile in sourceView.GetActiveTiles(activeTileBuffer))
                {
                    if (!dirtyMap.IsDirty(tile.X, tile.Y, zPosition))
                    {
                        continue;
                    }

                    if (!sourceView.TryGetData(tile.X, tile.Y, out var sourceTile) ||
                        !directionMap.TryGetWriteAccess(tile.X, tile.Y, out var resultTile, DataViewCreateMode.CreateMissing))
                    {
                        continue;
                    }

                    processingParameterCache.Add(new ProcessingParameters(tile, zPosition, sourceView, sourceTile, resultTile));
                }
            }

            if (processingParameterCache.Count == 0)
            {
                return false;
            }
            
            var r = Parallel.ForEach(processingParameterCache, processTileDelegate);
            return r.IsCompleted;
        }

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

                if (!TargetData.TryGetWritableView(zPosition, out var directionMap, DataViewCreateMode.CreateMissing))
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
                        !directionMap.TryGetWriteAccess(tile.X, tile.Y, out var resultTile, DataViewCreateMode.CreateMissing))
                    {
                        continue;
                    }

                    var effectiveArea = tile.GetIntersection(bounds);
                    processingParameterCache.Add(new ProcessingParameters(effectiveArea, zPosition, sourceView, sourceTile, resultTile));
                }
            }

            if (processingParameterCache.Count == 0)
            {
                return false;
            }
            
            var r = Parallel.ForEach(processingParameterCache, processTileDelegate);
            return r.IsCompleted;
        }

        protected abstract void ProcessTile(ProcessingParameters args);

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

        public void MarkCleanSystem<TGameContext>(TGameContext ctx)
        {
            dirtyMap.MarkClean();
        }
        
        protected readonly struct ProcessingParameters
        {
            public readonly Rectangle Bounds;
            public readonly int ZPosition;
            public readonly IReadOnlyDynamicDataView2D<TSourceData> SourceLayer;
            public readonly IReadOnlyBoundedDataView<TSourceData> SourceTile;
            public readonly IBoundedDataView<TTargetData> ResultTile;

            public ProcessingParameters(Rectangle bounds,
                                        int zPosition,
                                        IReadOnlyDynamicDataView2D<TSourceData> sourceLayer,
                                        IReadOnlyBoundedDataView<TSourceData> sourceTile,
                                        IBoundedDataView<TTargetData> resultTile)
            {
                Bounds = bounds;
                ZPosition = zPosition;
                SourceTile = sourceTile;
                ResultTile = resultTile;
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
                resultTile = ResultTile;
            }
        }
    }
    
    public abstract class GridTransformSystem<TSourceData, TTargetData>: GridTransformSystemBase<TSourceData, TTargetData>
    {
        protected GridTransformSystem(IReadOnlyDynamicDataView3D<TSourceData> sourceData): base(sourceData.ToConfiguration())
        {
            this.SourceData = sourceData;
            this.TargetData = new DynamicDataView3D<TTargetData>(sourceData.ToConfiguration());
        }

        protected override IReadOnlyDynamicDataView3D<TSourceData> SourceData { get; }
        protected override IDynamicDataView3D<TTargetData> TargetData { get; }

        public IReadOnlyDynamicDataView3D<TTargetData> ResultView => TargetData;
    }
}