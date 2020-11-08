using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class DynamicSenseLayerFactory<TGameContext, TItemId, TSense> : DynamicGridAggregateLayerFactoryBase<TGameContext,TItemId, SensoryResistance<TSense>>
        where TItemId : IEntityKey
    {
        readonly IItemContext<TGameContext, TItemId> itemContext;

        public DynamicSenseLayerFactory(MapLayer layer, 
                                        [NotNull] IGridMapContext<TItemId> mapContext,
                                        [NotNull] IItemContext<TGameContext, TItemId> itemContext): base(layer, mapContext)
        {
            this.itemContext = itemContext ?? throw new ArgumentNullException(nameof(itemContext));
        }

        protected override IAggregationPropertiesDataProcessor<TGameContext, SensoryResistance<TSense>> CreateDataProcessor(MapLayer layer, int zLayer, DynamicDataViewConfiguration config)
        {
            return new SensePropertiesDataProcessor<TGameContext, TItemId, TSense>(layer,
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