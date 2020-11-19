using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public static class SensePropertiesSystemExtensions
    {
        public static void AddLayer<TGameContext, TItemId, TSense>(this IAggregationLayerSystemBackend<TGameContext, SensoryResistance<TSense>> system,
                                                                   IGridMapContext<TItemId> mapContext,
                                                                   IItemResolver<TGameContext, TItemId> itemContext,
                                                                   MapLayer mapLayer)
            where TItemId : IEntityKey
        {
            system.AddSenseLayerFactory(new DynamicSenseLayerFactory<TGameContext, TItemId, TSense>(mapLayer, mapContext, itemContext));
        }
    }
}