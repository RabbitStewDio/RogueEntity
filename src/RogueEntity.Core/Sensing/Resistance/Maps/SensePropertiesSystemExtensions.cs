using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public static class SensePropertiesSystemExtensions
    {
        public static void AddLayer<TGameContext, TItemId, TSense>(this ISensePropertiesSystem<TGameContext, TSense> system, MapLayer mapLayer)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TItemId>
        {
            system.AddSenseLayerFactory(new DynamicSenseLayerFactory<TGameContext, TItemId, TSense>(mapLayer));
        }
    }
}