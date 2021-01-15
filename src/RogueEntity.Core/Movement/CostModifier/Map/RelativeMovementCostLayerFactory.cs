using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.CostModifier.Map
{
    public class RelativeMovementCostLayerFactory<TItemId, TSense> : DynamicGridAggregateLayerFactoryBase<TItemId, RelativeMovementCostModifier<TSense>>
        where TItemId : IEntityKey
    {
        readonly IItemResolver<TItemId> itemContext;

        public RelativeMovementCostLayerFactory(MapLayer layer,
                                                [NotNull] IGridMapContext<TItemId> mapContext,
                                                [NotNull] IItemResolver<TItemId> itemContext) : base(layer, mapContext)
        {
            this.itemContext = itemContext ?? throw new ArgumentNullException(nameof(itemContext));
        }

        protected override IAggregationPropertiesDataProcessor<RelativeMovementCostModifier<TSense>> CreateDataProcessor(MapLayer layer, int zLayer, DynamicDataViewConfiguration config)
        {
            return new RelativeMovementCostDataProcessor<TItemId, TSense>(layer,
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
