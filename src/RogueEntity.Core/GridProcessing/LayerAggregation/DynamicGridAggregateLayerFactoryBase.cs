using System;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public abstract class DynamicGridAggregateLayerFactoryBase<TItemId, TAggregateType> : IAggregationLayerController<TAggregateType>
        where TItemId : struct, IEntityKey
    {
        readonly MapLayer layer;
        readonly IGridMapContext<TItemId> mapContext;
        readonly BufferList<int> cachedZLevels;

        protected DynamicGridAggregateLayerFactoryBase(MapLayer layer, IGridMapContext<TItemId> mapContext)
        {
            this.layer = layer;
            this.mapContext = mapContext ?? throw new ArgumentNullException(nameof(mapContext));
            this.cachedZLevels = new BufferList<int>();
        }

        protected IGridMapContext<TItemId> MapContext => mapContext;

        public void Start(IAggregationLayerSystemBackend<TAggregateType> system)
        {
            if (!mapContext.TryGetGridDataFor(layer, out var gdc))
            {
                return;
            }

            gdc.PositionDirty += system.OnPositionDirty;
        }

        public void PrepareLayers(IAggregationLayerSystemBackend<TAggregateType> system)
        {
            if (!mapContext.TryGetGridDataFor(layer, out var gridMapDataContext))
            {
                return;
            }

            gridMapDataContext.GetActiveLayers(cachedZLevels);

            foreach (var z in cachedZLevels)
            {
                if (!gridMapDataContext.TryGetView(z, out _))
                {
                    // If the map no longer contains the z-layer we previously seen,
                    // kick it out from the system for good.
                    if (system.TryGetAggregationLayer(z, out var mlx))
                    {
                        mlx.RemoveLayer(layer);
                    }

                    continue;
                }

                var ml = system.GetOrCreate(z);
                if (!ml.IsDefined(layer))
                {
                    var proc = CreateDataProcessor(layer, z, system.ViewConfiguration);
                    ml.AddProcess(layer, proc);
                }
            }
        }

        protected abstract IAggregationPropertiesDataProcessor<TAggregateType> CreateDataProcessor(MapLayer layer, int zLayer, DynamicDataViewConfiguration config);

        public void Stop(IAggregationLayerSystemBackend<TAggregateType> system)
        {
            if (!mapContext.TryGetGridDataFor(layer, out var gdc))
            {
                return;
            }

            gdc.PositionDirty -= system.OnPositionDirty;
        }
    }
}
