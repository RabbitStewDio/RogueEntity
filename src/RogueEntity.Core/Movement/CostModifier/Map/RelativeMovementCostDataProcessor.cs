using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Movement.CostModifier.Map
{
    public class RelativeMovementCostDataProcessor< TItemId, TMovementMode> : GridAggregationPropertiesDataProcessor< TItemId, RelativeMovementCostModifier<TMovementMode>>
        where TItemId : struct, IEntityKey
    {
        readonly IItemResolver< TItemId> itemContext;

        public RelativeMovementCostDataProcessor(MapLayer layer,
                                               IMapContext<TItemId> mapContext,
                                               IItemResolver< TItemId> itemContext,
                                               int zPosition,
                                               int offsetX,
                                               int offsetY,
                                               int tileSizeX,
                                               int tileSizeY) : base(layer, mapContext, zPosition, offsetX, offsetY, tileSizeX, tileSizeY)
        {
            this.itemContext = itemContext;
        }

        protected override void ProcessTile(TileProcessingParameters p)
        {
            var (bounds, groundData, resultTile) = p;
            using var buffer = BufferListPool<(TItemId, EntityGridPosition)>.GetPooled();
            var itemResolver = itemContext;
            foreach (var (x, y) in bounds.Contents)
            {
                var itemCostOnTile = RelativeMovementCostModifier<TMovementMode>.Unchanged.CostModifier;
                foreach (var (item, _) in groundData.QueryItemTile<EntityGridPosition>(EntityGridPosition.OfRaw(Layer.LayerId, x, y, ZPosition), buffer))
                {
                    if (itemResolver.TryQueryData(item,  out RelativeMovementCostModifier<TMovementMode> itemCost))
                    {
                        if (itemCost.CostModifier > itemCostOnTile)
                        {
                            itemCostOnTile = itemCost.CostModifier;
                        }
                    }
                }

                resultTile.TrySet(x, y, new RelativeMovementCostModifier<TMovementMode>(itemCostOnTile));
            }
        }
    }
}