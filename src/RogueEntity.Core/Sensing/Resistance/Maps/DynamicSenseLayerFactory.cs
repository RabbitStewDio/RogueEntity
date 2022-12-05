using System;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class DynamicSenseLayerFactory<TItemId, TSense> : DynamicGridAggregateLayerFactoryBase<TItemId, SensoryResistance<TSense>>
        where TItemId : struct, IEntityKey
    {
        readonly IItemResolver<TItemId> itemContext;

        public DynamicSenseLayerFactory(MapLayer layer,
                                        IMapContext<TItemId> mapContext,
                                        IItemResolver<TItemId> itemContext) : base(layer, mapContext)
        {
            this.itemContext = itemContext ?? throw new ArgumentNullException(nameof(itemContext));
        }

        protected override IAggregationPropertiesDataProcessor<SensoryResistance<TSense>> CreateDataProcessor(MapLayer layer, int zLayer, DynamicDataViewConfiguration config)
        {
            return new SensePropertiesDataProcessor<TItemId, TSense>(layer,
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
