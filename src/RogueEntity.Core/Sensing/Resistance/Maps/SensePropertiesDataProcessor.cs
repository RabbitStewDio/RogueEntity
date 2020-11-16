using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesDataProcessor<TGameContext, TItemId, TSense> : GridAggregationPropertiesDataProcessor<TGameContext, TItemId, SensoryResistance<TSense>>
        where TItemId : IEntityKey
    {
        readonly IItemResolver<TGameContext, TItemId> itemContext;

        public SensePropertiesDataProcessor(MapLayer layer,
                                            [NotNull] IGridMapContext<TItemId> mapContext,
                                            [NotNull] IItemResolver<TGameContext, TItemId> itemContext,
                                            int zPosition,
                                            int offsetX,
                                            int offsetY,
                                            int tileSizeX,
                                            int tileSizeY) : base(layer, mapContext, zPosition, offsetX, offsetY, tileSizeX, tileSizeY)
        {
            this.itemContext = itemContext ?? throw new ArgumentNullException(nameof(itemContext));
        }

        protected override void ProcessTile(TileProcessingParameters p)
        {
            var (bounds, context, _, groundData, resultTile) = p;

            var itemResolver = itemContext;
            foreach (var (x, y) in bounds.Contents)
            {
                var groundItemRef = groundData[x, y];
                if (itemResolver.TryQueryData(groundItemRef, context, out SensoryResistance<TSense> groundItem))
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