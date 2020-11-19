using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Movement.CostModifier.Map
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "Discriminator")]
    public interface IRelativeMovementCostSystem<TGameContext, TMovementType>: IAggregationLayerSystem<TGameContext, float>
    {
        
    }
    
    public class RelativeMovementCostSystem<TGameContext, TMovementType> : LayeredAggregationSystem<TGameContext, float, RelativeMovementCostModifier<TMovementType>>,
                                                                           IRelativeMovementCostSystem<TGameContext, TMovementType>
    {
        public RelativeMovementCostSystem(int tileWidth, int tileHeight) : base(RelativeMovementCostSystem.ProcessTile, tileWidth, tileHeight)
        {
        }

        public RelativeMovementCostSystem(int offsetX, int offsetY, int tileSizeX, int tileSizeY) : base(RelativeMovementCostSystem.ProcessTile, offsetX, offsetY, tileSizeX, tileSizeY)
        {
        }
    }

    public static class RelativeMovementCostSystem
    {
        public static void ProcessTile<TMovementType>(AggregationProcessingParameter<float, RelativeMovementCostModifier<TMovementType>> p)
        {
            var bounds = p.Bounds;
            var resistanceData = p.WritableTile;
            foreach (var (x, y) in bounds.Contents)
            {
                var sp = RelativeMovementCostModifier<TMovementType>.Unchanged;
                foreach (var dv in p.DataViews)
                {
                    if (dv.TryGet(x, y, out var d))
                    {
                        sp *= d;
                    }
                }

                resistanceData.TrySet(x, y, sp);
            }
        }

        public static void AddLayer<TGameContext, TItemId, TMovementMode>(this IAggregationLayerSystemBackend<TGameContext, RelativeMovementCostModifier<TMovementMode>> system,
                                                                          IGridMapContext<TItemId> mapContext,
                                                                          IItemResolver<TGameContext, TItemId> itemContext,
                                                                          MapLayer mapLayer)
            where TItemId : IEntityKey
        {
            system.AddSenseLayerFactory(new RelativeMovementCostLayerFactory<TGameContext, TItemId, TMovementMode>(mapLayer, mapContext, itemContext));
        }
    }
}