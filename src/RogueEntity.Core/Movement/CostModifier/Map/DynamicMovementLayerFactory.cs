using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.CostModifier.Map
{
    public class DynamicMovementLayerFactory<TGameContext, TItemId, TSense> : DynamicGridAggregateLayerFactoryBase<TGameContext, TItemId, MovementCostModifier<TSense>>
        where TItemId : IEntityKey
    {
        readonly IItemContext<TGameContext, TItemId> itemContext;

        public DynamicMovementLayerFactory(MapLayer layer, 
                                           [NotNull] IGridMapContext<TItemId> mapContext,
                                           [NotNull] IItemContext<TGameContext, TItemId> itemContext): base(layer, mapContext)
        {
            this.itemContext = itemContext ?? throw new ArgumentNullException(nameof(itemContext));
        }

        protected override IAggregationPropertiesDataProcessor<TGameContext, MovementCostModifier<TSense>> CreateDataProcessor(MapLayer layer, int zLayer, DynamicDataViewConfiguration config)
        {
            return new MovementPropertiesDataProcessor<TGameContext, TItemId, TSense>(layer,
                                                                                      MapContext,
                                                                                      itemContext,
                                                                                      zLayer,
                                                                                      config.OffsetX,
                                                                                      config.OffsetY,
                                                                                      config.TileSizeX,
                                                                                      config.TileSizeY);
        }
    }
}