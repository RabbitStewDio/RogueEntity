using System;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance.Maps
{
    public class SensePropertiesDataProcessor<TItemId, TSense> : GridAggregationPropertiesDataProcessor<TItemId, SensoryResistance<TSense>>
        where TItemId : struct, IEntityKey
    {
        readonly IItemResolver<TItemId> itemContext;

        public SensePropertiesDataProcessor(MapLayer layer,
                                            IMapContext<TItemId> mapContext,
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
            var (bounds, groundData, resultTile) = p;

            using var buffer = BufferListPool<(TItemId, EntityGridPosition)>.GetPooled();
            var itemResolver = itemContext;
            foreach (var (x, y) in bounds.Contents)
            {

                var senseData = SensoryResistance<TSense>.Empty.BlocksSense.RawData;
                foreach (var (item, _) in groundData.QueryItemTile<EntityGridPosition>(EntityGridPosition.OfRaw(Layer.LayerId, x, y, ZPosition), buffer))
                {
                    if (!itemResolver.TryQueryData(item, out SensoryResistance<TSense> groundItem))
                    {
                        continue;
                    }

                    var raw = groundItem.BlocksSense.RawData;
                    if (raw > senseData)
                    {
                        senseData = raw;
                    }
                }

                resultTile.TrySet(x, y, new SensoryResistance<TSense>(Percentage.FromRaw(senseData)));
            }
        }
    }
}
