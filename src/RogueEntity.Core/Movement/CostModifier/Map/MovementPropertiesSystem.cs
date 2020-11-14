using EnTTSharp.Entities;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Movement.CostModifier.Map
{
    public class MovementPropertiesSystem<TGameContext, TMovementType> : LayeredAggregationSystem<TGameContext, MovementCostModifier<TMovementType>>
    {
        public MovementPropertiesSystem(int tileWidth, int tileHeight) : base(MovementPropertiesSystem.ProcessTile, tileWidth, tileHeight)
        {
        }

        public MovementPropertiesSystem(int offsetX, int offsetY, int tileSizeX, int tileSizeY) : base(MovementPropertiesSystem.ProcessTile, offsetX, offsetY, tileSizeX, tileSizeY)
        {
        }
    }

    public static class MovementPropertiesSystem
    {
        public static void ProcessTile<TMovementType>(AggregationProcessingParameter<MovementCostModifier<TMovementType>> p)
        {
            var bounds = p.Bounds;
            var resistanceData = p.WritableTile;
            foreach (var (x, y) in bounds.Contents)
            {
                var sp = new MovementCostModifier<TMovementType>();
                foreach (var dv in p.DataViews)
                {
                    if (dv.TryGet(x, y, out var d))
                    {
                        sp += d;
                    }
                }

                resistanceData.TrySet(x, y, sp);
            }
        }

        public static void AddLayer<TGameContext, TItemId, TSense>(this IAggregationLayerSystem<TGameContext, MovementCostModifier<TSense>> system, 
                                                                   IGridMapContext<TItemId> mapContext,
                                                                   IItemContext<TGameContext, TItemId> itemContext,
                                                                   MapLayer mapLayer)
            where TItemId : IEntityKey
        {
            system.AddSenseLayerFactory(new DynamicMovementLayerFactory<TGameContext, TItemId, TSense>(mapLayer, mapContext, itemContext));
        }
    }
}