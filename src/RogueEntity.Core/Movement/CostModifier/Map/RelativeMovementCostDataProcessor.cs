using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Movement.CostModifier.Map
{
    public class RelativeMovementCostDataProcessor< TItemId, TMovementMode> : GridAggregationPropertiesDataProcessor< TItemId, RelativeMovementCostModifier<TMovementMode>>
        where TItemId : struct, IEntityKey
    {
        readonly IItemResolver< TItemId> itemContext;

        public RelativeMovementCostDataProcessor(MapLayer layer,
                                               IGridMapContext<TItemId> mapContext,
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
            var (bounds, _, groundData, resultTile) = p;

            var itemResolver = itemContext;
            foreach (var (x, y) in bounds.Contents)
            {
                var groundItemRef = groundData[x, y];
                if (itemResolver.TryQueryData(groundItemRef,  out RelativeMovementCostModifier<TMovementMode> groundItem))
                {
                    resultTile.TrySet(x, y, in groundItem);
                }
                else
                {
                    resultTile.TrySet(x, y, RelativeMovementCostModifier<TMovementMode>.Unchanged);
                }
            }
        }
    }
}