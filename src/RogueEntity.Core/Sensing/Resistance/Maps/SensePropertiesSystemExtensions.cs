using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public static class SensePropertiesSystemExtensions
    {
        public static void AddLayer<TGameContext, TItemId>(this ISensePropertiesSystem<TGameContext> system, MapLayer mapLayer)
            where TItemId : IEntityKey
            where TGameContext : IItemContext<TGameContext, TItemId>, IGridMapContext<TGameContext, TItemId>, IGridMapRawDataContext<TItemId>
        {
            system.AddSenseLayerFactory(new DynamicSenseLayerFactory<TGameContext, TItemId>(mapLayer));
        }
    }
}