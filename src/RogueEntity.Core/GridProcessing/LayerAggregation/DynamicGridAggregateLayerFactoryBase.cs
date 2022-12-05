using System;
using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public abstract class DynamicGridAggregateLayerFactoryBase<TItemId, TAggregateType> : IAggregationLayerController<TAggregateType>
        where TItemId : struct, IEntityKey
    {
        readonly MapLayer layer;
        readonly IMapContext<TItemId> mapContext;
        readonly BufferList<int> cachedZLevels;

        protected DynamicGridAggregateLayerFactoryBase(MapLayer layer, IMapContext<TItemId> mapContext)
        {
            this.layer = layer;
            this.mapContext = mapContext ?? throw new ArgumentNullException(nameof(mapContext));
            this.cachedZLevels = new BufferList<int>();
        }

        protected IMapContext<TItemId> MapContext => mapContext;

        public void Start(IAggregationLayerSystemBackend<TAggregateType> system)
        {
            if (!mapContext.TryGetMapDataFor(layer, out var gdc))
            {
                return;
            }

            gdc.PositionDirty += system.OnPositionDirty;
        }

        public void PrepareLayers(IAggregationLayerSystemBackend<TAggregateType> system)
        {
            if (!mapContext.TryGetMapDataFor(layer, out var gridMapDataContext))
            {
                return;
            }

            using var zBuffer = BufferListPool<int>.GetPooled();
            gridMapDataContext.GetActiveZLayers(cachedZLevels);

            foreach (var z in system.GetActiveLayers(zBuffer))
            {
                if (!cachedZLevels.Contains(z))
                {
                    if (system.TryGetAggregationLayer(z, out var mlx))
                    {
                        mlx.RemoveLayer(layer);
                    }
                }
            }

            foreach (var z in cachedZLevels)
            {
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
            if (!mapContext.TryGetMapDataFor(layer, out var gdc))
            {
                return;
            }

            gdc.PositionDirty -= system.OnPositionDirty;
        }
    }
}