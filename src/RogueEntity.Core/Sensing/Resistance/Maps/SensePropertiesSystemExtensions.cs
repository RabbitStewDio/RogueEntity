using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public static class SensePropertiesSystemExtensions
    {
        public static void AddLayer< TItemId, TSense>(this IAggregationLayerSystemBackend< SensoryResistance<TSense>> system,
                                                                   IGridMapContext<TItemId> mapContext,
                                                                   IItemResolver< TItemId> itemContext,
                                                                   MapLayer mapLayer)
            where TItemId : struct, IEntityKey
        {
            system.AddSenseLayerFactory(new DynamicSenseLayerFactory< TItemId, TSense>(mapLayer, mapContext, itemContext));
        }
    }
}