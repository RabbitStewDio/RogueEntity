using System;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesDataProcessor<TItemId, TSense> : GridAggregationPropertiesDataProcessor<TItemId, SensoryResistance<TSense>>
        where TItemId : struct, IEntityKey
    {
        readonly IItemResolver<TItemId> itemContext;

        public SensePropertiesDataProcessor(MapLayer layer,
                                            IGridMapContext<TItemId> mapContext,
                                            IItemResolver<TItemId> itemContext,
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
            var (bounds, _, groundData, resultTile) = p;

            var itemResolver = itemContext;
            foreach (var (x, y) in bounds.Contents)
            {
                if (!groundData.TryGet(x, y, out var groundItemRef))
                {
                    // no data at this tile?    
                    // undefined tiles acts as blocking. This ensures that there is a hard border around the defined map
                    resultTile.TrySet(x, y, SensoryResistance<TSense>.Blocked);
                }
                
                if (itemResolver.TryQueryData(groundItemRef, out SensoryResistance<TSense> groundItem))
                {
                    resultTile.TrySet(x, y, in groundItem);
                }
                else
                {
                    // undefined acts as blocking. This ensures that there is a hard border around the defined map
                    resultTile.TrySet(x, y, SensoryResistance<TSense>.Empty);
                }
            }
        }
    }
}
