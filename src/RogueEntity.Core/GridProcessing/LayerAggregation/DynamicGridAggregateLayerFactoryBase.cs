using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public abstract class DynamicGridAggregateLayerFactoryBase<TGameContext, TItemId, TAggregateType> : IAggregationLayerController<TGameContext, TAggregateType>
        where TItemId : IEntityKey
        where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TItemId>
    {
        readonly MapLayer layer;
        readonly List<int> cachedZLevels;

        public DynamicGridAggregateLayerFactoryBase(MapLayer layer)
        {
            this.layer = layer;
            this.cachedZLevels = new List<int>();
        }

        public void Start(TGameContext context, IAggregationLayerSystem<TGameContext, TAggregateType> system)
        {
            if (!context.TryGetGridDataFor(layer, out var gdc))
            {
                return;
            }

            gdc.PositionDirty += system.OnPositionDirty;
        }

        public void PrepareLayers(TGameContext context, IAggregationLayerSystem<TGameContext, TAggregateType> system)
        {
            if (!context.TryGetGridDataFor(layer, out var gridMapDataContext))
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
                    if (system.TryGetSenseLayer(z, out var mlx))
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

        protected abstract IAggregationPropertiesDataProcessor<TGameContext, TAggregateType> CreateDataProcessor(MapLayer layer, int zLayer, DynamicDataViewConfiguration config);

        public void Stop(TGameContext context, IAggregationLayerSystem<TGameContext, TAggregateType> system)
        {
            if (!context.TryGetGridDataFor(layer, out var gdc))
            {
                return;
            }

            gdc.PositionDirty -= system.OnPositionDirty;
        }
    }
}