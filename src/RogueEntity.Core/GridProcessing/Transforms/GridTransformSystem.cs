using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Caching;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.Transforms
{
    public abstract class GridTransformSystem<TSourceData, TTargetData> : IReadOnlyDynamicDataView3D<TTargetData>
    {
        public event EventHandler<DynamicDataView3DEventArgs<TTargetData>> ViewCreated;
        public event EventHandler<DynamicDataView3DEventArgs<TTargetData>> ViewExpired;
        
        readonly IReadOnlyDynamicDataView3D<TSourceData> sourceData;
        readonly Dictionary<int, DynamicDataView2D<TTargetData>> targetData;
        readonly GridTileStateView dirtyMap;
        readonly List<ProcessingParameters> processingParameterCache;
        readonly List<Rectangle> activeTileBuffer;
        readonly List<int> activeZLevelsBuffer;
        readonly Action<ProcessingParameters> processTileDelegate;

        protected GridTransformSystem(IReadOnlyDynamicDataView3D<TSourceData> sourceData)
        {
            this.sourceData = sourceData;
            this.targetData = new Dictionary<int, DynamicDataView2D<TTargetData>>();
            this.dirtyMap = new GridTileStateView(sourceData.OffsetX, sourceData.OffsetY, sourceData.TileSizeX, sourceData.TileSizeY);
            this.processingParameterCache = new List<ProcessingParameters>();
            this.activeTileBuffer = new List<Rectangle>();
            this.activeZLevelsBuffer = new List<int>();
            this.processTileDelegate = ProcessTile;
        }

        public int OffsetX
        {
            get { return sourceData.OffsetX; }
        }

        public int OffsetY
        {
            get { return sourceData.OffsetY; }
        }

        public int TileSizeX
        {
            get { return sourceData.TileSizeX; }
        }

        public int TileSizeY
        {
            get { return sourceData.TileSizeY; }
        }

        public List<int> GetActiveLayers(List<int> buffer = null)
        {
            if (buffer == null)
            {
                buffer = new List<int>();
            }
            else
            {
                buffer.Clear();
            }

            foreach (var d in targetData.Keys)
            {
                buffer.Add(d);
            }

            return buffer;
        }

        public bool TryGetView(int z, out IReadOnlyDynamicDataView2D<TTargetData> view)
        {
            if (targetData.TryGetValue(z, out var viewRaw))
            {
                view = viewRaw;
                return true;
            }

            view = default;
            return false;
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

        DynamicDataView2D<TTargetData> GetWritableMap(int z)
        {
            if (targetData.TryGetValue(z, out var m))
            {
                return m;
            }

            m = new DynamicDataView2D<TTargetData>(sourceData.OffsetX, sourceData.OffsetY, sourceData.TileSizeX, sourceData.TileSizeY);
            targetData[z] = m;
            return m;
        }

        public void Process()
        {
            processingParameterCache.Clear();

            foreach (var zPosition in sourceData.GetActiveLayers(activeZLevelsBuffer))
            {
                if (!sourceData.TryGetView(zPosition, out var sourceView))
                {
                    continue;
                }

                var directionMap = GetWritableMap(zPosition);
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

            Parallel.ForEach(processingParameterCache, processTileDelegate);
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
    }
}