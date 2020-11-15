using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Movement.CostModifier.Map
{
    public class MovementPropertiesDataProcessor<TGameContext, TItemId, TSense> : GridAggregationPropertiesDataProcessor<TGameContext, TItemId, MovementCostModifier<TSense>>
        where TItemId : IEntityKey
    {
        [NotNull] readonly IItemResolver<TGameContext, TItemId> itemContext;

        public MovementPropertiesDataProcessor(MapLayer layer,
                                               [NotNull] IGridMapContext<TItemId> mapContext,
                                               [NotNull] IItemResolver<TGameContext, TItemId> itemContext,
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
            var (bounds, context, _, groundData, resultTile) = p;

            var itemResolver = itemContext;
            foreach (var (x, y) in bounds.Contents)
            {
                var groundItemRef = groundData[x, y];
                if (itemResolver.TryQueryData(groundItemRef, context, out MovementCostModifier<TSense> groundItem))
                {
                    resultTile.TrySet(x, y, in groundItem);
                }
                else
                {
                    resultTile.TrySet(x, y, default);
                }
            }
        }
    }
}